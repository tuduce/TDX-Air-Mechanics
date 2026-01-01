using System;
using System.Diagnostics;
using System.Linq;
using SharpDX.DirectInput;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services.Effects
{
    public class GroundVibrationEffect : IEffect
    {
        private Joystick? _joystick;
        private AirplaneProfile? _profile;
        private Effect? _groundEffect1;
        private Effect? _groundEffect2;
        private Effect? _groundJoltEffect;
        private double _groundDistanceAccumM = 0;
        private long _lastGroundTick = 0;

        private Joystick? GetJoystickSafe() { var js = _joystick; if (js == null) return null; try { if (js.NativePointer == IntPtr.Zero) return null; return js; } catch { return null; } }
        public void AttachDevice(Joystick joystick) { _joystick = joystick; }
        public void DetachDevice() { Reset(); _joystick = null; }
        public void ApplyProfile(AirplaneProfile? profile) { _profile = profile; if (_profile?.GroundVibration != true) Reset(); }
        public void Reset() { Remove(); _groundDistanceAccumM = 0; _lastGroundTick = 0; }
        public void Start() { }
        public void Stop() { try { _groundEffect1?.Stop(); } catch { } try { _groundEffect2?.Stop(); } catch { } try { _groundJoltEffect?.Stop(); } catch { } }
        public void Dispose() { Reset(); }

        public void Update(SimVariableData data)
        {
            var js = GetJoystickSafe(); if (js == null || _profile?.GroundVibration != true) return;
            bool onGround = data.OnGround >= 0.5; if (!onGround) { Remove(); return; }
            EnsureOrUpdate(js, data);
        }

        private void EnsureOrUpdate(Joystick js, SimVariableData data)
        {
            try
            {
                var axisObjects = js.GetObjects(DeviceObjectTypeFlags.ForceFeedbackActuator).OrderBy(a => a.Usage).ToList();
                if (axisObjects.Count == 0) axisObjects = js.GetObjects(DeviceObjectTypeFlags.Axis).Where(a => a.Usage == 48 || a.Usage == 49).OrderBy(a => a.Usage).ToList();
                if (axisObjects.Count == 0) return;
                int[] axes = axisObjects.Select(a => a.Offset).ToArray(); int[] dirs = new int[axes.Length];
                var periodicInfo = js.GetEffects(EffectType.Periodic).FirstOrDefault();
                var constantInfo = js.GetEffects(EffectType.ConstantForce).FirstOrDefault();
                if (periodicInfo == null) return;
                double gs = Math.Max(0, data.GroundSpeed);
                long now = Environment.TickCount64; if (_lastGroundTick == 0) _lastGroundTick = now; double dtSec = Math.Max(0.01, (now - _lastGroundTick) / 1000.0); _lastGroundTick = now; _groundDistanceAccumM += gs * dtSec;
                int surface = (int)Math.Round(data.GroundType);
                bool isConcrete = surface == 1;
                bool isGrass = surface == 2;
                bool isAsphalt = surface == 4 || surface == 18;
                double speedNorm = Math.Clamp(gs / 50.0, 0.0, 1.0);
                if (_groundEffect1 == null)
                {
                    var ep1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0 };
                    ep1.SetAxes(axes, dirs); ep1.Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 0, Period = 80000 };
                    _groundEffect1 = new Effect(js, periodicInfo.Guid, ep1); _groundEffect1.Start(1);
                }
                if (_groundEffect2 == null)
                {
                    var ep2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0 };
                    ep2.SetAxes(axes, dirs); ep2.Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 18000, Period = 60000 };
                    _groundEffect2 = new Effect(js, periodicInfo.Guid, ep2); _groundEffect2.Start(1);
                }
                if (isAsphalt)
                {
                    if (gs < 0.5)
                    {
                        var off1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 0, Period = 14000 } };
                        off1.SetAxes(axes, dirs); _groundEffect1?.SetParameters(off1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                        var off2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 9000, Period = 22000 } };
                        off2.SetAxes(axes, dirs); _groundEffect2?.SetParameters(off2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    }
                    else
                    {
                        int mag1 = (int)Math.Round(240 * speedNorm);
                        int mag2 = (int)Math.Round(180 * speedNorm);
                        int per1 = 14000;
                        int per2 = 22000;
                        var upd1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = mag1, Offset = 0, Phase = 0, Period = per1 } };
                        upd1.SetAxes(axes, dirs); _groundEffect1?.SetParameters(upd1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                        var upd2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = mag2, Offset = 0, Phase = 9000, Period = per2 } };
                        upd2.SetAxes(axes, dirs); _groundEffect2?.SetParameters(upd2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    }
                }
                else if (isGrass)
                {
                    if (gs < 0.5)
                    {
                        var off1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 0, Period = 120000 } };
                        off1.SetAxes(axes, dirs); _groundEffect1?.SetParameters(off1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                        var off2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 18000, Period = 90000 } };
                        off2.SetAxes(axes, dirs); _groundEffect2?.SetParameters(off2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    }
                    else
                    {
                        int per1 = (int)Math.Round(120000 - 60000 * speedNorm);
                        int per2 = (int)Math.Round(90000 - 45000 * speedNorm);
                        int mag1 = (int)Math.Round(1400 * speedNorm);
                        int mag2 = (int)Math.Round(1000 * speedNorm);
                        var upd1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = mag1, Offset = 0, Phase = 0, Period = per1 } };
                        upd1.SetAxes(axes, dirs); _groundEffect1?.SetParameters(upd1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                        var upd2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = mag2, Offset = 0, Phase = 18000, Period = per2 } };
                        upd2.SetAxes(axes, dirs); _groundEffect2?.SetParameters(upd2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    }
                }
                else if (isConcrete)
                {
                    int bgMag = (int)Math.Round(140 * speedNorm);
                    int bgPer = 30000;
                    var updBg = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = bgMag, Offset = 0, Phase = 0, Period = bgPer } };
                    updBg.SetAxes(axes, dirs); _groundEffect1?.SetParameters(updBg, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    var upd2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 18000, Period = 28000 } };
                    upd2.SetAxes(axes, dirs); _groundEffect2?.SetParameters(upd2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    if (constantInfo != null && _groundDistanceAccumM >= 20.0)
                    {
                        _groundDistanceAccumM -= 20.0;
                        if (_groundJoltEffect == null)
                        {
                            var epJ = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = 50000, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0 };
                            epJ.SetAxes(axes, dirs); epJ.Parameters = new ConstantForce { Magnitude = 3000 };
                            _groundJoltEffect = new Effect(js, constantInfo.Guid, epJ);
                        }
                        int pulseMag = (int)Math.Round(2000 + 4000 * speedNorm);
                        var updJ = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = 50000, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new ConstantForce { Magnitude = pulseMag } };
                        updJ.SetAxes(axes, dirs); _groundJoltEffect.SetParameters(updJ, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start); _groundJoltEffect.Start(1);
                    }
                }
                else
                {
                    var upd1 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 0, Period = 80000 } };
                    upd1.SetAxes(axes, dirs); _groundEffect1?.SetParameters(upd1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                    var upd2 = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = 0, Offset = 0, Phase = 18000, Period = 60000 } };
                    upd2.SetAxes(axes, dirs); _groundEffect2?.SetParameters(upd2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[GroundVibrationEffect] Failed to ensure/update");
                Remove();
            }
        }

        private void Remove()
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
        }
    }
}
