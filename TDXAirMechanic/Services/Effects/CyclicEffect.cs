using System;
using System.Diagnostics;
using System.Linq;
using SharpDX.DirectInput;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services.Effects
{
    // Helicopter cyclic effect: separate spring and damper condition effects on both axes
    public class CyclicEffect : IEffect
    {
        private Joystick? _joystick;
        private AirplaneProfile? _profile;
        private Effect? _springEffect;
        private Effect? _damperEffect;
        private int[]? _axes;
        private short[]? _axisUsages;
        private int _springCoeff = 0;   // 0..10000
        private int _dampingCoeff = 0;  // 0..10000
        private int _centerOffsetX = 0; // trim-like offsets for cyclic center
        private int _centerOffsetY = 0;
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
        }

        public void DetachDevice()
        {
            Reset();
            _joystick = null;
        }

        public void ApplyProfile(AirplaneProfile? profile)
        {
            _profile = profile;
            if (_profile?.CyclicEnabled == true && _enabled)
                EnsureEffects();
            else
                RemoveEffects();
        }

        public void Update(SimVariableData data)
        {
            var js = GetJoystickSafe();
            if (js == null || _profile?.CyclicEnabled != true || !_enabled) return;
            // Parameters are driven by UI; no sim-variable coupling yet
        }

        public void Reset()
        {
            RemoveEffects();
            _centerOffsetX = 0;
            _centerOffsetY = 0;
        }

        // Reset cyclic center to origin (0,0)
        public void ResetCenter()
        {
            _centerOffsetX = 0;
            _centerOffsetY = 0;
            ApplyParams();
        }

        public void Start() { }
        public void Stop() { try { _springEffect?.Stop(); } catch { } try { _damperEffect?.Stop(); } catch { } }
        public void Dispose() { Reset(); }

        private static bool IsSpring(EffectInfo info)
        {
            var name = info?.Name ?? string.Empty;
            return name.Contains("Spring", StringComparison.OrdinalIgnoreCase) || name.Contains("GUID_Spring", StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsDamper(EffectInfo info)
        {
            var name = info?.Name ?? string.Empty;
            return name.Contains("Damper", StringComparison.OrdinalIgnoreCase) || name.Contains("GUID_Damper", StringComparison.OrdinalIgnoreCase);
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!_enabled)
            {
                try { _springEffect?.Stop(); } catch { }
                try { _damperEffect?.Stop(); } catch { }
            }
            else
            {
                EnsureEffects();
                ApplyParams();
            }
        }

        private void EnsureEffects()
        {
            var js = GetJoystickSafe();
            if (js == null) return;
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
                _axes = axes;
                _axisUsages = axisObjects.Select(a => a.Usage).ToArray();

                // Find specific condition effect GUIDs
                var condInfos = js.GetEffects(EffectType.Condition).ToList();
                var springInfo = condInfos.FirstOrDefault(IsSpring);
                var damperInfo = condInfos.FirstOrDefault(IsDamper);

                // Create spring effect
                if (_springEffect == null && springInfo != null)
                {
                    var epSpring = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    epSpring.SetAxes(axes, dirs);

                    var springSet = new ConditionSet { Conditions = new Condition[axes.Length] };
                    for (int i = 0; i < axes.Length; i++)
                    {
                        int usage = _axisUsages != null && i < _axisUsages.Length ? _axisUsages[i] : (short)0;
                        int offset = (usage == 48) ? _centerOffsetX : (usage == 49) ? _centerOffsetY : 0;
                        springSet.Conditions[i] = new Condition
                        {
                            Offset = offset,
                            PositiveCoefficient = _springCoeff,
                            NegativeCoefficient = _springCoeff,
                            DeadBand = 0,
                            PositiveSaturation = 10000,
                            NegativeSaturation = 10000
                        };
                    }
                    epSpring.Parameters = springSet;
                    _springEffect = new Effect(js, springInfo.Guid, epSpring);
                    _springEffect.Start(1);
                }

                // Create damper effect
                if (_damperEffect == null && damperInfo != null)
                {
                    var epDamper = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    epDamper.SetAxes(axes, dirs);

                    var damperSet = new ConditionSet { Conditions = new Condition[axes.Length] };
                    for (int i = 0; i < axes.Length; i++)
                    {
                        damperSet.Conditions[i] = new Condition
                        {
                            Offset = 0,
                            PositiveCoefficient = _dampingCoeff,
                            NegativeCoefficient = _dampingCoeff,
                            DeadBand = 0,
                            PositiveSaturation = 10000,
                            NegativeSaturation = 10000
                        };
                    }
                    epDamper.Parameters = damperSet;
                    _damperEffect = new Effect(js, damperInfo.Guid, epDamper);
                    _damperEffect.Start(1);
                }

                ApplyParams();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[CyclicEffect] Failed to create cyclic effects");
                RemoveEffects();
            }
        }

        private void ApplyParams()
        {
            var js = GetJoystickSafe();
            if (js == null || _axes == null || _axes.Length == 0) return;

            try
            {
                int[] axes = _axes;
                int[] dirs = new int[axes.Length];

                // Update spring
                if (_springEffect != null)
                {
                    var updSpring = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    updSpring.SetAxes(axes, dirs);

                    var springSet = new ConditionSet { Conditions = new Condition[axes.Length] };
                    for (int i = 0; i < axes.Length; i++)
                    {
                        int usage = _axisUsages != null && i < _axisUsages.Length ? _axisUsages[i] : (short)0;
                        int offset = (usage == 48) ? _centerOffsetX : (usage == 49) ? _centerOffsetY : 0;
                        springSet.Conditions[i] = new Condition
                        {
                            Offset = offset,
                            PositiveCoefficient = _springCoeff,
                            NegativeCoefficient = _springCoeff,
                            DeadBand = 0,
                            PositiveSaturation = 10000,
                            NegativeSaturation = 10000
                        };
                    }
                    updSpring.Parameters = springSet;
                    _springEffect.SetParameters(updSpring, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                }

                // Update damper
                if (_damperEffect != null)
                {
                    var updDamper = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    updDamper.SetAxes(axes, dirs);

                    var damperSet = new ConditionSet { Conditions = new Condition[axes.Length] };
                    for (int i = 0; i < axes.Length; i++)
                    {
                        damperSet.Conditions[i] = new Condition
                        {
                            Offset = 0,
                            PositiveCoefficient = _dampingCoeff,
                            NegativeCoefficient = _dampingCoeff,
                            DeadBand = 0,
                            PositiveSaturation = 10000,
                            NegativeSaturation = 10000
                        };
                    }
                    updDamper.Parameters = damperSet;
                    _damperEffect.SetParameters(updDamper, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[CyclicEffect] Failed to apply params");
            }
        }

        private void RemoveEffects()
        {
            try { _springEffect?.Stop(); } catch { }
            try { _springEffect?.Dispose(); } catch { }
            _springEffect = null;
            try { _damperEffect?.Stop(); } catch { }
            try { _damperEffect?.Dispose(); } catch { }
            _damperEffect = null;
            _axes = null;
            _axisUsages = null;
        }

        // Public API to set UI-driven values (0..100) mapped to 0..10000
        public void SetSpringPercent(int percent)
        {
            _springCoeff = Math.Clamp(percent, 0, 100) * 100;
            ApplyParams();
        }

        public void SetDampingPercent(int percent)
        {
            _dampingCoeff = Math.Clamp(percent, 0, 100) * 100;
            ApplyParams();
        }

        // New: set cyclic center offsets based on raw stick position
        public void SetCenterFromRaw(int rawX, int rawY)
        {
            if (_profile?.CyclicEnabled != true) return;
            try
            {
                int center = 32767;
                int range = 32767;
                double nx = Math.Clamp((rawX - center) / (double)range, -1.0, 1.0);
                double ny = Math.Clamp((rawY - center) / (double)range, -1.0, 1.0);
                // allow trim all the way on helicopters
                int max = 10000;// Math.Max(0, _profile?.MaxTrimOffset ?? 4000);
                _centerOffsetX = Math.Clamp((int)(nx * max), -max, max);
                _centerOffsetY = Math.Clamp((int)(ny * max), -max, max);
                ApplyParams();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[CyclicEffect] Failed to set center from raw stick");
            }
        }
    }
}
