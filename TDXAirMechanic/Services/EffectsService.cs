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
        private Effect? _gearVibrationEffect1; // first sine wave for gear down vibration
        private Effect? _gearVibrationEffect2; // second sine wave for gear down vibration
        private Effect? _groundEffect1;       // ground roll base vibration
        private Effect? _groundEffect2;       // secondary ground roll vibration or jolt carrier
        private Effect? _groundJoltEffect;    // short pulse effect for concrete

        // Cache axes used for spring updates and last applied stiffness to avoid churn
        private int[]? _springAxes;
        private short[]? _springAxisUsages; // track usages to map X/Y
        private int _lastSpringCoeff = -1;

        // Trim offsets per axis (aligned with _springAxisUsages order)
        private int _trimOffsetX = 0; // Usage 48
        private int _trimOffsetY = 0; // Usage 49

        // Ground roll state
        private double _groundDistanceAccumM = 0; // meters since last jolt on concrete
        private long _lastGroundTick = 0;         // ms

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

            // Gear vibration should be stopped if disabled in profile
            if (_profile?.GearVibration != true)
                RemoveGearVibrationEffects();

            // Ground vibration should be stopped if disabled in profile
            if (_profile?.GroundVibration != true)
                RemoveGroundEffects();
        }

        public void Update(SimVariableData data)
        {
            var js = GetJoystickSafe();
            if (js == null || _profile == null)
                return;

            // Ensure base spring exists when requested
            if (_profile.CenteredSpring)
                EnsureSpringEffect(js);

            // Dynamic spring tuning based on airspeed
            if (_profile.CenteredSpring && _profile.DynamicSpring)
                UpdateDynamicSpring(js, data);

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

            bool onGround = data.OnGround >= 0.5;

            // Gear vibration when gear is down, and aircraft not on ground
            if (_profile.GearVibration)
            {
                bool gearDown = data.GearPosition >= 0.5; // threshold
                if (gearDown && !onGround)
                {
                    EnsureGearVibrationEffects(js, data);
                }
                else
                {
                    RemoveGearVibrationEffects();
                }
            }

            // Ground roll vibrations when on ground
            if (_profile.GroundVibration)
            {
                if (onGround)
                {
                    EnsureOrUpdateGroundEffects(js, data);
                }
                else
                {
                    RemoveGroundEffects();
                }
            }
            else
            {
                RemoveGroundEffects();
            }
        }

        public void ResetAll()
        {
            RemoveStickShakerEffect();
            RemoveGearVibrationEffects();
            RemoveGroundEffects();
            RemoveSpringEffect();
            _trimOffsetX = 0;
            _trimOffsetY = 0;
        }

        public void NudgeTrim(int pitchDelta, int rollDelta)
        {
            // Only applicable when spring exists and centered spring is active
            if (_springEffect == null || _profile?.CenteredSpring != true)
                return;

            try
            {
                // Update internal trim offsets
                int max = Math.Max(0, _profile?.MaxTrimOffset ?? 4000);
                if (rollDelta != 0)
                {
                    _trimOffsetX = Math.Clamp(_trimOffsetX + rollDelta, -max, max);
                }
                if (pitchDelta != 0)
                {
                    _trimOffsetY = Math.Clamp(_trimOffsetY + pitchDelta, -max, max);
                }

                ApplySpringOffsets();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[Effects] Failed to apply trim nudge");
            }
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
                _springAxes = axes;
                _springAxisUsages = axisObjects.Select(a => a.Usage).ToArray();

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
                        DeadBand = 0,
                        PositiveSaturation = 10000,
                        NegativeSaturation = 10000
                    };
                }

                ep.Parameters = cs;

                _springEffect = new Effect(js, springInfo.Guid, ep);
                _springEffect.Start(1);
                _lastSpringCoeff = -1; // force first dynamic update to apply
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[Effects] Failed to create spring effect");
                RemoveSpringEffect();
            }
        }

        private void ApplySpringOffsets()
        {
            if (_springEffect == null || _springAxes == null || _springAxes.Length == 0)
                return;

            var js = GetJoystickSafe();
            if (js == null)
                return;

            try
            {
                int[] axes = _springAxes;
                int[] dirs = new int[axes.Length];

                // Keep last stiffness if known; otherwise use default
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
                    int offset = 0;
                    if (usage == 48) // X
                        offset = _trimOffsetX;
                    else if (usage == 49) // Y
                        offset = _trimOffsetY;

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
                Debug.WriteLine(ex + "[Effects] Failed to apply spring offsets");
            }
        }

        private void UpdateDynamicSpring(Joystick js, SimVariableData data)
        {
            if (_springEffect == null)
                return;

            try
            {
                // Determine normalized speed factor [0..1]
                double ias = Math.Max(0, data.IAS);
                double barber = data.Barber;
                double factor = 0;
                if (barber > 0)
                {
                    factor = Math.Clamp(ias / barber, 0.0, 1.0);
                }
                else
                {
                    // Fallback heuristic if barber pole not provided: assume 250 KIAS as high end on many aircraft
                    factor = Math.Clamp(ias / 250.0, 0.0, 1.0);
                }

                // Map to stiffness range [min..max]
                const int minCoeff = 1000; // softer feel at low speed
                const int maxCoeff = 10000; // device max
                int coeff = (int)Math.Round(minCoeff + factor * (maxCoeff - minCoeff));

                // Avoid excessive updates; re-apply only when changed meaningfully
                if (Math.Abs(coeff - _lastSpringCoeff) < 100)
                {
                    // Still ensure offsets are honored if present
                    ApplySpringOffsets();
                    return;
                }

                var axes = _springAxes;
                if (axes == null || axes.Length == 0)
                {
                    // Try to recover axes if cache was lost
                    var objs = js.GetObjects(DeviceObjectTypeFlags.ForceFeedbackActuator)
                                 .OrderBy(a => a.Usage)
                                 .ToList();
                    if (objs.Count == 0)
                        objs = js.GetObjects(DeviceObjectTypeFlags.Axis).Where(a => a.Usage == 48 || a.Usage == 49).OrderBy(a => a.Usage).ToList();
                    axes = objs.Select(a => a.Offset).ToArray();
                    _springAxes = axes;
                    _springAxisUsages = objs.Select(a => a.Usage).ToArray();
                }
                if (axes == null || axes.Length == 0)
                    return;

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
                    int offset = 0;
                    if (usage == 48) offset = _trimOffsetX; // X
                    else if (usage == 49) offset = _trimOffsetY; // Y

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
                Debug.WriteLine(ex + "[Effects] Failed to update dynamic spring");
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
            _trimOffsetX = 0;
            _trimOffsetY = 0;
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

        private void EnsureGearVibrationEffects(Joystick js, SimVariableData data)
        {
            try
            {
                var periodicInfo = js.GetEffects(EffectType.Periodic).FirstOrDefault();
                if (periodicInfo == null)
                {
                    Debug.WriteLine("[Effects] Periodic effect not supported for gear vibration.");
                    return;
                }

                // Discover axes
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

                // Compute speed factor [0..1] using barber pole if available, else 250 KIAS reference
                double ias = Math.Max(0, data.IAS);
                double refSpeed = (data.Barber > 0) ? data.Barber : 250.0;
                double factor = refSpeed > 0 ? Math.Clamp(ias / refSpeed, 0.0, 1.0) : 0.0;

                // Map to magnitudes (max 1500 and 500 respectively)
                int mag1 = (int)Math.Round(500 * factor);
                int mag2 = (int)Math.Round(300 * factor);

                // Wave 1: lower frequency, magnitude up to 1500
                if (_gearVibrationEffect1 == null)
                {
                    var ep1 = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    ep1.SetAxes(axes, dirs);
                    ep1.Parameters = new PeriodicForce
                    {
                        Magnitude = mag1,
                        Offset = 0,
                        Phase = 0,
                        Period = 250000 // us
                    };
                    _gearVibrationEffect1 = new Effect(js, periodicInfo.Guid, ep1);
                    _gearVibrationEffect1.Start(1);
                }
                else
                {
                    var update1 = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0,
                        Parameters = new PeriodicForce
                        {
                            Magnitude = mag1,
                            Offset = 0,
                            Phase = 0,
                            Period = 300000 // us
                        }
                    };
                    update1.SetAxes(axes, dirs);
                    _gearVibrationEffect1.SetParameters(update1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                }

                // Wave 2: higher frequency, magnitude up to 500
                if (_gearVibrationEffect2 == null)
                {
                    var ep2 = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    ep2.SetAxes(axes, dirs);
                    ep2.Parameters = new PeriodicForce
                    {
                        Magnitude = mag2,
                        Offset = 0,
                        Phase = 18000, // phase shift to avoid synchronous peaks
                        Period = 22000 // 22 ms
                    };
                    _gearVibrationEffect2 = new Effect(js, periodicInfo.Guid, ep2);
                    _gearVibrationEffect2.Start(1);
                }
                else
                {
                    var update2 = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0,
                        Parameters = new PeriodicForce
                        {
                            Magnitude = mag2,
                            Offset = 0,
                            Phase = 18000,
                            Period = 22000
                        }
                    };
                    update2.SetAxes(axes, dirs);
                    _gearVibrationEffect2.SetParameters(update2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[Effects] Failed to ensure/update gear vibration");
                RemoveGearVibrationEffects();
            }
        }

        private void EnsureOrUpdateGroundEffects(Joystick js, SimVariableData data)
        {
            try
            {
                // Discover axes once per call to be robust to device changes
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

                // Effect capabilities
                var periodicInfo = js.GetEffects(EffectType.Periodic).FirstOrDefault();
                var constantInfo = js.GetEffects(EffectType.ConstantForce).FirstOrDefault();
                if (periodicInfo == null)
                {
                    // No periodic support -> cannot render ground vibrations meaningfully
                    return;
                }

                // Timing and speed
                double gs = Math.Max(0, data.GroundSpeed); // m/s
                long now = Environment.TickCount64;
                if (_lastGroundTick == 0) _lastGroundTick = now;
                double dtSec = Math.Max(0.01, (now - _lastGroundTick) / 1000.0);
                _lastGroundTick = now;
                _groundDistanceAccumM += gs * dtSec;

                // Surface mapping
                int surface = (int)Math.Round(data.GroundType);
                bool isConcrete = surface == 1;                   // CONCRETE
                bool isGrass = surface == 2;                      // GRASS
                bool isAsphalt = surface == 4 || surface == 18;   // ASPHALT or TARMAC

                // General normalized speed [0..1] for scaling
                double speedNorm = Math.Clamp(gs / 50.0, 0.0, 1.0); // 50 m/s ~ 97 kts

                // Ensure/create base periodic ground effects (two layers)
                if (_groundEffect1 == null)
                {
                    var ep1 = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    ep1.SetAxes(axes, dirs);
                    ep1.Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 0, Period = 80000 };
                    _groundEffect1 = new Effect(js, periodicInfo.Guid, ep1);
                    _groundEffect1.Start(1);
                }
                if (_groundEffect2 == null)
                {
                    var ep2 = new EffectParameters
                    {
                        Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                        Duration = int.MaxValue,
                        SamplePeriod = 0,
                        Gain = 10000,
                        TriggerButton = -1,
                        TriggerRepeatInterval = 0
                    };
                    ep2.SetAxes(axes, dirs);
                    ep2.Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 18000, Period = 60000 };
                    _groundEffect2 = new Effect(js, periodicInfo.Guid, ep2);
                    _groundEffect2.Start(1);
                }

                // Prepare updates per surface
                if (isAsphalt)
                {
                    // Minimal high-frequency vibrations, only when rolling
                    if (gs < 0.5)
                    {
                        var off1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 0, Period = 14000 } };
                        off1.SetAxes(axes, dirs);
                        _groundEffect1?.SetParameters(off1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                        var off2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 9000, Period = 22000 } };
                        off2.SetAxes(axes, dirs);
                        _groundEffect2?.SetParameters(off2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    }
                    else
                    {
                        int mag1 = (int)Math.Round(240 * speedNorm);
                        int mag2 = (int)Math.Round(180 * speedNorm);
                        int per1 = 14000; // 14 ms
                        int per2 = 22000; // 22 ms

                        var upd1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = mag1, Offset = 0, Phase = 0, Period = per1 } };
                        upd1.SetAxes(axes, dirs);
                        _groundEffect1?.SetParameters(upd1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);

                        var upd2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = mag2, Offset = 0, Phase = 9000, Period = per2 } };
                        upd2.SetAxes(axes, dirs);
                        _groundEffect2?.SetParameters(upd2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    }
                }
                else if (isGrass)
                {
                    // Low-frequency, moderate amplitude; frequency increases with speed; 0 when stopped
                    if (gs < 0.5)
                    {
                        var off1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 0, Period = 120000 } };
                        off1.SetAxes(axes, dirs);
                        _groundEffect1?.SetParameters(off1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                        var off2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 18000, Period = 90000 } };
                        off2.SetAxes(axes, dirs);
                        _groundEffect2?.SetParameters(off2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    }
                    else
                    {
                        int per1 = (int)Math.Round(120000 - 60000 * speedNorm); // 120ms -> 60ms
                        int per2 = (int)Math.Round(90000 - 45000 * speedNorm);  // 90ms -> 45ms
                        int mag1 = (int)Math.Round(1400 * speedNorm);          // 0..1400
                        int mag2 = (int)Math.Round(1000 * speedNorm);          // 0..1000

                        var upd1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = mag1, Offset = 0, Phase = 0, Period = per1 } };
                        upd1.SetAxes(axes, dirs);
                        _groundEffect1?.SetParameters(upd1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);

                        var upd2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = mag2, Offset = 0, Phase = 18000, Period = per2 } };
                        upd2.SetAxes(axes, dirs);
                        _groundEffect2?.SetParameters(upd2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    }
                }
                else if (isConcrete)
                {
                    // Background light vibration + concrete jolts every 20m; 0 background when stopped
                    int bgMag = (int)Math.Round(140 * speedNorm);
                    int bgPer = 30000; // 30 ms

                    var updBg = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = bgMag, Offset = 0, Phase = 0, Period = bgPer } };
                    updBg.SetAxes(axes, dirs);
                    _groundEffect1?.SetParameters(updBg, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);

                    // Keep second periodic very low to avoid masking jolts
                    var upd2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 18000, Period = 28000 } };
                    upd2.SetAxes(axes, dirs);
                    _groundEffect2?.SetParameters(upd2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);

                    // Fire a short jolt if enough distance accumulated
                    if (constantInfo != null && _groundDistanceAccumM >= 20.0)
                    {
                        // Consume one stride (approximate single pulse per update)
                        _groundDistanceAccumM -= 20.0;

                        if (_groundJoltEffect == null)
                        {
                            var epJ = new EffectParameters
                            {
                                Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                                Duration = 50000, // 50 ms
                                SamplePeriod = 0,
                                Gain = 10000,
                                TriggerButton = -1,
                                TriggerRepeatInterval = 0
                            };
                            epJ.SetAxes(axes, dirs);
                            epJ.Parameters = new ConstantForce { Magnitude = 3000 };
                            _groundJoltEffect = new Effect(js, constantInfo.Guid, epJ);
                        }

                        // Update magnitude scaled with speed and retrigger
                        int pulseMag = (int)Math.Round(2000 + 4000 * speedNorm); // 2k..6k small jolts
                        var updJ = new EffectParameters
                        {
                            Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                            Duration = 50000,
                            SamplePeriod = 0,
                            Gain = 10000,
                            TriggerButton = -1,
                            TriggerRepeatInterval = 0,
                            Parameters = new ConstantForce { Magnitude = pulseMag }
                        };
                        updJ.SetAxes(axes, dirs);
                        _groundJoltEffect.SetParameters(updJ, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                        _groundJoltEffect.Start(1);
                    }
                }
                else
                {
                    // Unknown/unsupported surface: keep minimal vibrations off
                    var upd1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 0, Period = 80000 } };
                    upd1.SetAxes(axes, dirs);
                    _groundEffect1?.SetParameters(upd1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);

                    var upd2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 18000, Period = 60000 } };
                    upd2.SetAxes(axes, dirs);
                    _groundEffect2?.SetParameters(upd2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[Effects] Failed to ensure/update ground effects");
                RemoveGroundEffects();
            }
        }

        private void RemoveGearVibrationEffects()
        {
            try { _gearVibrationEffect1?.Stop(); } catch { }
            try { _gearVibrationEffect1?.Dispose(); } catch { }
            _gearVibrationEffect1 = null;

            try { _gearVibrationEffect2?.Stop(); } catch { }
            try { _gearVibrationEffect2?.Dispose(); } catch { }
            _gearVibrationEffect2 = null;
        }

        private void RemoveGroundEffects()
        {
            try { _groundEffect1?.Stop(); } catch { }
            try { _groundEffect1?.Dispose(); } catch { }
            _groundEffect1 = null;

            try { _groundEffect2?.Stop(); } catch { }
            try { _groundEffect2?.Dispose(); } catch { }
            _groundEffect2 = null;

            try { _groundJoltEffect?.Stop(); } catch { }
            try { _groundJoltEffect?.Dispose(); } catch { }
            _groundJoltEffect = null;

            _groundDistanceAccumM = 0;
            _lastGroundTick = 0;
        }

        public void Dispose()
        {
            ResetAll();
        }
    }
}
