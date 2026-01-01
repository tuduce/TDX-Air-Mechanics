using System;
using System.Diagnostics;
using System.Linq;
using SharpDX.DirectInput;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services.Effects
{
    public class StickShakerEffect : IEffect
    {
        private Joystick? _joystick;
        private AirplaneProfile? _profile;
        private Effect? _effect;

        private Joystick? GetJoystickSafe()
        { var js = _joystick; if (js == null) return null; try { if (js.NativePointer == IntPtr.Zero) return null; return js; } catch { return null; } }

        public void AttachDevice(Joystick joystick) { _joystick = joystick; }
        public void DetachDevice() { Reset(); _joystick = null; }

        public void ApplyProfile(AirplaneProfile? profile)
        {
            _profile = profile;
            if (_profile?.StickShaker != true) Reset();
        }

        public void Update(SimVariableData data)
        {
            var js = GetJoystickSafe(); if (js == null || _profile?.StickShaker != true) return;
            bool stall = data.StallWarning > 0.5;
            bool overspeed = data.IAS > 0 && data.Barber > 0 && data.IAS >= data.Barber * 0.98;
            if (!(stall || overspeed)) { Remove(); return; }
            EnsureAndUpdate(js, stall, overspeed);
        }

        public void Reset() { Remove(); }
        public void Start() { }
        public void Stop() { try { _effect?.Stop(); } catch { } }
        public void Dispose() { Reset(); }

        private void EnsureAndUpdate(Joystick js, bool stall, bool overspeed)
        {
            try
            {
                if (_effect == null)
                {
                    var axisObjects = js.GetObjects(DeviceObjectTypeFlags.ForceFeedbackActuator).OrderBy(a => a.Usage).ToList();
                    if (axisObjects.Count == 0) axisObjects = js.GetObjects(DeviceObjectTypeFlags.Axis).Where(a => a.Usage == 48 || a.Usage == 49).OrderBy(a => a.Usage).ToList();
                    if (axisObjects.Count == 0) return;
                    int[] axes = axisObjects.Select(a => a.Offset).ToArray();
                    int[] dirs = new int[axes.Length];
                    var sineInfo = js.GetEffects(EffectType.Periodic).FirstOrDefault();
                    if (sineInfo == null) return;
                    var ep = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0 };
                    ep.SetAxes(axes, dirs);
                    ep.Parameters = new PeriodicForce { Magnitude = 3000, Offset = 0, Phase = 0, Period = 30000 };
                    _effect = new Effect(js, sineInfo.Guid, ep);
                    _effect.Start(1);
                }
                int magnitude = stall ? 9000 : 6000;
                int period = stall ? 20000 : 35000;
                var update = new EffectParameters { Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian, Duration = int.MaxValue, SamplePeriod = 0, Gain = 10000, TriggerButton = -1, TriggerRepeatInterval = 0, Parameters = new PeriodicForce { Magnitude = magnitude, Offset = 0, Phase = 0, Period = period } };
                try
                {
                    var objs = js.GetObjects(DeviceObjectTypeFlags.ForceFeedbackActuator).ToList();
                    if (objs.Count == 0) objs = js.GetObjects(DeviceObjectTypeFlags.Axis).Where(a => a.Usage == 48 || a.Usage == 49).ToList();
                    if (objs.Count > 0) { int[] axes = objs.Select(a => a.Offset).ToArray(); int[] dirs = new int[axes.Length]; update.SetAxes(axes, dirs); }
                }
                catch { }
                _effect?.SetParameters(update, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[StickShakerEffect] Failed to ensure/update");
                Remove();
            }
        }

        private void Remove()
        {
            try { _effect?.Stop(); } catch { }
            try { _effect?.Dispose(); } catch { }
            _effect = null;
        }
    }
}
