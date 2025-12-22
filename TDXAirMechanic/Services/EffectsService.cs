using System;
using System.Diagnostics;
using System.Linq;
using SharpDX.DirectInput;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services
{
    // Manages lifecycle of FFB effects on the currently attached joystick
    public class EffectsService : IEffectsService
    {
        private Joystick? _joystick;
        private AirplaneProfile? _profile;

        private Effect? _springEffect;
        private Effect? _stickShakerEffect; // single periodic effect applied on X/Y

        private Joystick? GetJoystickSafe()
        {
            var js = _joystick;
            if (js == null)
                return null;
            try
            {
                // If disposed, NativePointer is zero and any call will throw
                if (js.NativePointer == IntPtr.Zero)
                    return null;
                return js;
            }
            catch
            {
                return null;
            }
        }

        public void AttachDevice(Joystick joystick)
        {
            if (ReferenceEquals(_joystick, joystick))
                return;

            DetachDevice();
            _joystick = joystick;

            // Re-apply profile to new device
            if (_profile != null)
            {
                ApplyProfile(_profile);
            }
        }

        public void DetachDevice()
        {
            ResetAll();
            _joystick = null;
        }

        public void ApplyProfile(AirplaneProfile? profile)
        {
            _profile = profile;

            var js = GetJoystickSafe();
            if (js == null)
                return;

            // Spring
            if (_profile?.CenteredSpring == true)
                EnsureSpringEffect(js);
            else
                RemoveSpringEffect();

            // Stick shaker (lazy-created on Update when actually needed)
            if (_profile?.StickShaker != true)
                RemoveStickShakerEffect();
        }

        public void Update(SimVariableData data)
        {
            var js = GetJoystickSafe();
            if (js == null || _profile == null)
                return;

            // Ensure base spring exists when requested
            if (_profile.CenteredSpring)
                EnsureSpringEffect(js);

            // Stick shaker based on stall warning or overspeed
            if (_profile.StickShaker)
            {
                bool stall = data.StallWarning > 0.5;
                bool overspeed = data.IAS > 0 && data.Barber > 0 && data.IAS >= data.Barber * 0.98; // near barber pole
                if (stall || overspeed)
                {
                    EnsureStickShakerEffect(js, stall, overspeed);
                }
                else
                {
                    RemoveStickShakerEffect();
                }
            }
        }

        public void ResetAll()
        {
            RemoveStickShakerEffect();
            RemoveSpringEffect();
        }

        private void EnsureSpringEffect(Joystick js)
        {
            if (js == null)
                return;

            if (_springEffect != null)
                return; // already created

            try
            {
                // Try to use actuator objects first
                var axisObjects = js.GetObjects(DeviceObjectTypeFlags.ForceFeedbackActuator)
                    .OrderBy(a => a.Usage)
                    .ToList();

                if (axisObjects.Count == 0)
                {
                    axisObjects = js.GetObjects(DeviceObjectTypeFlags.Axis)
                        .Where(a => a.Usage == 48 || a.Usage == 49)
                        .OrderBy(a => a.Usage)
                        .ToList();
                    Debug.WriteLine($"[Effects] Fallback axis discovery. Count={axisObjects.Count}");
                }

                if (axisObjects.Count == 0)
                {
                    Debug.WriteLine("[Effects] No axes found for spring effect.");
                    return;
                }

                int[] axes = axisObjects.Select(a => a.Offset).ToArray();
                int[] dirs = new int[axes.Length];

                var springInfo = js.GetEffects(EffectType.Condition).FirstOrDefault();
                if (springInfo == null)
                {
                    Debug.WriteLine("[Effects] Spring effect not supported by device.");
                    return;
                }

                var ep = new EffectParameters
                {
                    Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                    Duration = int.MaxValue,
                    SamplePeriod = 0,
                    Gain = 10000,
                    TriggerButton = -1,
                    TriggerRepeatInterval = 0
                };
                ep.SetAxes(axes, dirs);

                var cs = new ConditionSet
                {
                    Conditions = new Condition[axes.Length]
                };
                for (int i = 0; i < axes.Length; i++)
                {
                    cs.Conditions[i] = new Condition
                    {
                        Offset = 0,
                        PositiveCoefficient = 10000,
                        NegativeCoefficient = 10000,
                        DeadBand = 500,
                        PositiveSaturation = 10000,
                        NegativeSaturation = 10000
                    };
                }

                ep.Parameters = cs;

                _springEffect = new Effect(js, springInfo.Guid, ep);
                _springEffect.Start(1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[Effects] Failed to create spring effect");
                RemoveSpringEffect();
            }
        }

        private void RemoveSpringEffect()
        {
            try { _springEffect?.Stop(); } catch { }
            try { _springEffect?.Dispose(); } catch { }
            _springEffect = null;
        }

        private void EnsureStickShakerEffect(Joystick js, bool stall, bool overspeed)
        {
            if (js == null)
                return;

            try
            {
                // Create lazily if needed
                if (_stickShakerEffect == null)
                {
                    var axisObjects = js.GetObjects(DeviceObjectTypeFlags.ForceFeedbackActuator)
                        .OrderBy(a => a.Usage)
                        .ToList();

                    if (axisObjects.Count == 0)
                    {
                        axisObjects = js.GetObjects(DeviceObjectTypeFlags.Axis)
                            .Where(a => a.Usage == 48 || a.Usage == 49)
                            .OrderBy(a => a.Usage)
                            .ToList();
                    }

                    if (axisObjects.Count == 0)
                        return;

                    int[] axes = axisObjects.Select(a => a.Offset).ToArray();
                    int[] dirs = new int[axes.Length];

                    var sineInfo = js.GetEffects(EffectType.Periodic).FirstOrDefault();
                    if (sineInfo == null)
                    {
                        Debug.WriteLine("[Effects] Periodic effect not supported for stick shaker.");
                        return;
                    }

                    var ep = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    ep.SetAxes(axes, dirs);

                    // Base periodic params; will be updated depending on state
                    var periodic = new PeriodicForce
                    {
                        Magnitude = 3000,
                        Offset = 0,
                        Phase = 0,
                        Period = 30000 // in microseconds
                    };

                    ep.Parameters = periodic;

                    _stickShakerEffect = new Effect(js, sineInfo.Guid, ep);
                    _stickShakerEffect.Start(1);
                }

                // Update magnitude/frequency based on stall vs overspeed
                int magnitude = stall ? 9000 : 6000;
                int period = stall ? 20000 : 35000;

                var update = new EffectParameters
                {
                    Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                    Duration = int.MaxValue,
                    SamplePeriod = 0,
                    Gain = 10000,
                    TriggerButton = -1,
                    TriggerRepeatInterval = 0,
                    Parameters = new PeriodicForce
                    {
                        Magnitude = magnitude,
                        Offset = 0,
                        Phase = 0,
                        Period = period
                    }
                };

                // Keep axes as initially set
                // SharpDX requires re-setting axes when updating parameters on some drivers
                try
                {
                    var objs = js.GetObjects(DeviceObjectTypeFlags.ForceFeedbackActuator).ToList();
                    if (objs.Count == 0)
                        objs = js.GetObjects(DeviceObjectTypeFlags.Axis).Where(a => a.Usage == 48 || a.Usage == 49).ToList();
                    if (objs.Count > 0)
                    {
                        int[] axes = objs.Select(a => a.Offset).ToArray();
                        int[] dirs = new int[axes.Length];
                        update.SetAxes(axes, dirs);
                    }
                }
                catch { }

                _stickShakerEffect?.SetParameters(update, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start); // Start to apply immediately
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[Effects] Failed to ensure/update stick shaker");
                RemoveStickShakerEffect();
            }
        }

        private void RemoveStickShakerEffect()
        {
            try { _stickShakerEffect?.Stop(); } catch { }
            try { _stickShakerEffect?.Dispose(); } catch { }
            _stickShakerEffect = null;
        }

        public void Dispose()
        {
            ResetAll();
        }
    }
}
