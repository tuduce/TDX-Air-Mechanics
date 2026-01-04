using SharpDX.DirectInput;
using System.Diagnostics;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services.Effects
{
    public class GearVibrationEffect : IEffect
    {
        private Joystick? _joystick;
        private AirplaneProfile? _profile;
        private Effect? _effect1;
        private Effect? _effect2;

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
            if (_profile?.GearVibration != true)
                Reset();
        }

        public void Reset()
        {
            Remove();
        }

        public void Start() { }

        public void Stop()
        {
            try
            {
                _effect1?.Stop();
            }
            catch
            {
            }

            try
            {
                _effect2?.Stop();
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            Reset();
        }

        public void Update(SimVariableData data)
        {
            var js = GetJoystickSafe();
            if (js == null || _profile?.GearVibration != true)
                return;

            bool gearDown = data.GearPosition >= 0.5;
            bool onGround = data.OnGround >= 0.5;

            if (!(gearDown && !onGround))
            {
                Remove();
                return;
            }

            EnsureOrUpdate(js, data);
        }

        private void EnsureOrUpdate(Joystick js, SimVariableData data)
        {
            try
            {
                var periodicInfo = js.GetEffects(EffectType.Periodic).FirstOrDefault();
                if (periodicInfo == null)
                    return;

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

                double ias = Math.Max(0, data.IAS);
                double refSpeed = (data.Barber > 0) ? data.Barber : 250.0;
                double factor = refSpeed > 0 ? Math.Clamp(ias / refSpeed, 0.0, 1.0) : 0.0;

                int mag1 = (int)Math.Round(300 * factor);
                int mag2 = (int)Math.Round(300 * factor);

                if (_effect1 == null)
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
                        Period = 16670
                    };
                    _effect1 = new Effect(js, periodicInfo.Guid, ep1);
                    _effect1.Start(1);
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
                            Period = 16670
                        }
                    };
                    update1.SetAxes(axes, dirs);
                    _effect1.SetParameters(update1, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                }

                if (_effect2 == null)
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
                        Phase = 18000,
                        Period = 22000
                    };
                    _effect2 = new Effect(js, periodicInfo.Guid, ep2);
                    _effect2.Start(1);
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
                    _effect2.SetParameters(update2, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.Start);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[GearVibrationEffect] Failed to ensure/update");
                Remove();
            }
        }

        private void Remove()
        {
            try { _effect1?.Stop(); } catch { }
            try { _effect1?.Dispose(); } catch { }
            _effect1 = null;

            try { _effect2?.Stop(); } catch { }
            try { _effect2?.Dispose(); } catch { }
            _effect2 = null;
        }
    }
}
