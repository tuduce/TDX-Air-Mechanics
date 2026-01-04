using System;
using System.Diagnostics;
using System.Linq;
using SharpDX.DirectInput;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services.Effects
{
    // Centered + Dynamic spring with trim offsets
    public class SpringEffect : IEffect
    {
        private Joystick? _joystick;
        private AirplaneProfile? _profile;
        private Effect? _springEffect;
        private int[]? _springAxes;
        private short[]? _springAxisUsages;
        private int _lastSpringCoeff = -1;
        private int _trimOffsetX = 0;
        private int _trimOffsetY = 0;
        private bool _enabled = true;

        private Joystick? GetJoystickSafe()
        {
            var js = _joystick;
            if (js == null) return null;
            try
            {
                if (js.NativePointer == IntPtr.Zero) return null;
                return js;
            }
            catch
            {
                return null;
            }
        }

        public void AttachDevice(Joystick joystick)
        {
            _joystick = joystick;
            // reapply profile
            if (_profile?.CenteredSpring == true && _enabled)
                EnsureSpringEffect();
        }

        public void DetachDevice()
        {
            Reset();
            _joystick = null;
        }

        public void ApplyProfile(AirplaneProfile? profile)
        {
            _profile = profile;
            if (_profile?.CenteredSpring == true && _enabled)
                EnsureSpringEffect();
            else
                RemoveSpringEffect();
        }

        public void Update(SimVariableData data)
        {
            var js = GetJoystickSafe();
            if (js == null || _profile == null) return;

            if (_profile.CenteredSpring && _enabled)
                EnsureSpringEffect();

            if (_profile.CenteredSpring && _profile.DynamicSpring && _enabled)
                UpdateDynamicSpring(data);
        }

        public void Reset()
        {
            RemoveSpringEffect();
        }

        public void Start()
        {
            // spring runs continuously when created
        }

        public void Stop()
        {
            try { _springEffect?.Stop(); } catch { }
        }

        public void Dispose()
        {
            Reset();
        }

        public void NudgeTrim(int pitchDelta, int rollDelta)
        {
            if (_springEffect == null || _profile?.CenteredSpring != true) return;
            try
            {
                int max = Math.Max(0, _profile?.MaxTrimOffset ?? 4000);
                if (rollDelta != 0)
                    _trimOffsetX = Math.Clamp(_trimOffsetX + rollDelta, -max, max);
                if (pitchDelta != 0)
                    _trimOffsetY = Math.Clamp(_trimOffsetY + pitchDelta, -max, max);
                ApplySpringOffsets();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[SpringEffect] Failed to apply trim nudge");
            }
        }

        // New: reset trim offsets to center
        public void ResetTrimOffsets()
        {
            _trimOffsetX = 0;
            _trimOffsetY = 0;
            ApplySpringOffsets();
        }

        // New: temporarily enable/disable spring without losing current trim
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!_enabled)
            {
                // Stop effect but keep offsets in memory
                try { _springEffect?.Stop(); } catch { }
            }
            else
            {
                // Recreate/start effect with current offsets
                EnsureSpringEffect();
                ApplySpringOffsets();
            }
        }

        private void EnsureSpringEffect()
        {
            var js = GetJoystickSafe();
            if (js == null || _springEffect != null) return;
            try
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
                _springAxes = axes;
                _springAxisUsages = axisObjects.Select(a => a.Usage).ToArray();

                var springInfo = js.GetEffects(EffectType.Condition).FirstOrDefault();
                if (springInfo == null)
                    return;

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
                        DeadBand = 0,
                        PositiveSaturation = 10000,
                        NegativeSaturation = 10000
                    };
                }
                ep.Parameters = cs;
                _springEffect = new Effect(js, springInfo.Guid, ep);
                _springEffect.Start(1);
                _lastSpringCoeff = -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[SpringEffect] Failed to create spring effect");
                RemoveSpringEffect();
            }
        }

        private void ApplySpringOffsets()
        {
            if (_springEffect == null || _springAxes == null || _springAxes.Length == 0) return;
            var js = GetJoystickSafe();
            if (js == null) return;
            try
            {
                int[] axes = _springAxes;
                int[] dirs = new int[axes.Length];
                int coeff = _lastSpringCoeff > 0 ? _lastSpringCoeff : 10000;

                var update = new EffectParameters
                {
                    Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                    Duration = int.MaxValue,
                    SamplePeriod = 0,
                    Gain = 10000,
                    TriggerButton = -1,
                    TriggerRepeatInterval = 0
                };
                update.SetAxes(axes, dirs);

                var cs = new ConditionSet
                {
                    Conditions = new Condition[axes.Length]
                };
                for (int i = 0; i < axes.Length; i++)
                {
                    int usage = _springAxisUsages != null && i < _springAxisUsages.Length ? _springAxisUsages[i] : (short)0;
                    int offset = (usage == 48) ? _trimOffsetX : (usage == 49) ? _trimOffsetY : 0;

                    cs.Conditions[i] = new Condition
                    {
                        Offset = offset,
                        PositiveCoefficient = coeff,
                        NegativeCoefficient = coeff,
                        DeadBand = 0,
                        PositiveSaturation = 10000,
                        NegativeSaturation = 10000
                    };
                }
                update.Parameters = cs;
                _springEffect.SetParameters(update, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[SpringEffect] Failed to apply spring offsets");
            }
        }

        private void UpdateDynamicSpring(SimVariableData data)
        {
            if (_springEffect == null) return;
            try
            {
                double ias = Math.Max(0, data.IAS);
                double barber = Math.Clamp(data.Barber, 200, 400);

                const int maxCoeff = 10000;

                int coeff = (int)(maxCoeff / (1 + Math.Pow(Math.E, (-1 * (5 / barber) * (ias - (barber / 2))))));

                if (Math.Abs(coeff - _lastSpringCoeff) < 100)
                {
                    ApplySpringOffsets();
                    return;
                }

                var axes = _springAxes;
                if (axes == null || axes.Length == 0)
                {
                    var js = GetJoystickSafe();
                    if (js == null) return;

                    var objs = js.GetObjects(DeviceObjectTypeFlags.ForceFeedbackActuator)
                        .OrderBy(a => a.Usage)
                        .ToList();

                    if (objs.Count == 0)
                    {
                        objs = js.GetObjects(DeviceObjectTypeFlags.Axis)
                            .Where(a => a.Usage == 48 || a.Usage == 49)
                            .OrderBy(a => a.Usage)
                            .ToList();
                    }

                    axes = objs.Select(a => a.Offset).ToArray();
                    _springAxes = axes;
                    _springAxisUsages = objs.Select(a => a.Usage).ToArray();
                }

                if (axes == null || axes.Length == 0) return;

                int[] dirs = new int[axes.Length];

                var update = new EffectParameters
                {
                    Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                    Duration = int.MaxValue,
                    SamplePeriod = 0,
                    Gain = 10000,
                    TriggerButton = -1,
                    TriggerRepeatInterval = 0
                };
                update.SetAxes(axes, dirs);

                var cs = new ConditionSet
                {
                    Conditions = new Condition[axes.Length]
                };
                for (int i = 0; i < axes.Length; i++)
                {
                    int usage = _springAxisUsages != null && i < _springAxisUsages.Length ? _springAxisUsages[i] : (short)0;
                    int offset = (usage == 48) ? _trimOffsetX : (usage == 49) ? _trimOffsetY : 0;

                    cs.Conditions[i] = new Condition
                    {
                        Offset = offset,
                        PositiveCoefficient = coeff,
                        NegativeCoefficient = coeff,
                        DeadBand = 0,
                        PositiveSaturation = 10000,
                        NegativeSaturation = 10000
                    };
                }
                update.Parameters = cs;
                _springEffect.SetParameters(update, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                _lastSpringCoeff = coeff;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[SpringEffect] Failed to update dynamic spring");
            }
        }

        private void RemoveSpringEffect()
        {
            try { _springEffect?.Stop(); } catch { }
            try { _springEffect?.Dispose(); } catch { }
            _springEffect = null;
            _springAxes = null;
            _springAxisUsages = null;
            _lastSpringCoeff = -1;
            // keep trim values; they get applied on next Ensure
        }

        // Expose trim to service
        public int TrimOffsetX => _trimOffsetX;
        public int TrimOffsetY => _trimOffsetY;

        // Set trim center based on raw device X/Y positions
        public void SetTrimCenterRaw(int rawX, int rawY)
        {
            if (_profile?.CenteredSpring != true) return;
            try
            {
                // DirectInput usually returns 0..65535. Map to condition offset range roughly -10000..+10000 centered at mid.
                int center = 32767; // approximate mid
                int range = 32767;  // normalize to [-1,1]
                double nx = Math.Clamp((rawX - center) / (double)range, -1.0, 1.0);
                double ny = Math.Clamp((rawY - center) / (double)range, -1.0, 1.0);

                int max = Math.Max(0, _profile?.MaxTrimOffset ?? 4000);
                _trimOffsetX = Math.Clamp((int)(nx * max), -max, max);
                _trimOffsetY = Math.Clamp((int)(ny * max), -max, max);
                ApplySpringOffsets();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[SpringEffect] Failed to set trim center from raw stick");
            }
        }
    }
}
