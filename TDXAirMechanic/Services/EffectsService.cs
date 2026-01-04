using SharpDX.DirectInput;
using TDXAirMechanic.Model;
using TDXAirMechanic.Services.Effects;

namespace TDXAirMechanic.Services
{
    // Orchestrates lifecycle of FFB effects via dedicated effect classes
    public class EffectsService : IEffectsService
    {
        private Joystick? _joystick;
        private AirplaneProfile? _profile;

        private readonly SpringEffect _spring = new();
        private readonly StickShakerEffect _shaker = new();
        private readonly GearVibrationEffect _gear = new();
        private readonly GroundVibrationEffect _ground = new();
        private readonly CyclicEffect _cyclic = new();

        private Joystick? GetJoystickSafe()
        {
            var js = _joystick;
            if (js == null) return null;
            try { if (js.NativePointer == IntPtr.Zero) return null; return js; } catch { return null; }
        }

        public void AttachDevice(Joystick joystick)
        {
            if (ReferenceEquals(_joystick, joystick)) return;
            DetachDevice();
            _joystick = joystick;
            _spring.AttachDevice(joystick);
            _shaker.AttachDevice(joystick);
            _gear.AttachDevice(joystick);
            _ground.AttachDevice(joystick);
            _cyclic.AttachDevice(joystick);
            if (_profile != null) ApplyProfile(_profile);
        }

        public void DetachDevice()
        {
            ResetAll();
            _spring.DetachDevice();
            _shaker.DetachDevice();
            _gear.DetachDevice();
            _ground.DetachDevice();
            _cyclic.DetachDevice();
            _joystick = null;
        }

        public void ApplyProfile(AirplaneProfile? profile)
        {
            _profile = profile;

            // Mutually exclusive: cyclic vs centered spring
            if (profile?.CyclicEnabled == true)
            {
                // Disable spring and apply cyclic
                _spring.SetEnabled(false);
                _cyclic.ApplyProfile(profile);
            }
            else
            {
                // Apply centered spring
                _spring.SetEnabled(true);
                _spring.ApplyProfile(profile);
                _cyclic.ApplyProfile(profile); // this will remove if disabled
            }

            _shaker.ApplyProfile(profile);
            _gear.ApplyProfile(profile);
            _ground.ApplyProfile(profile);

            // Seed cyclic parameters from profile
            if (profile != null)
            {
                _cyclic.SetSpringPercent(profile.CyclicSpring);
                _cyclic.SetDampingPercent(profile.CyclicDamping);
            }
        }

        public void Update(SimVariableData data)
        {
            if (GetJoystickSafe() == null || _profile == null) return;
            _spring.Update(data);
            _shaker.Update(data);
            _gear.Update(data);
            _ground.Update(data);
            _cyclic.Update(data);
        }

        public void ResetAll()
        {
            _shaker.Reset();
            _gear.Reset();
            _ground.Reset();
            _cyclic.Reset();
            _spring.Reset();
        }

        public void NudgeTrim(int pitchDelta, int rollDelta)
        {
            _spring.NudgeTrim(pitchDelta, rollDelta);
        }

        public void ResetTrim()
        {
            if (_profile?.CyclicEnabled == true)
            {
                _cyclic.ResetCenter();
            }
            else
            {
                _spring.ResetTrimOffsets();
            }
        }

        public void SetSpringEnabled(bool enabled)
        {
            // Respect active mode: disable/enable the right effect
            if (_profile?.CyclicEnabled == true)
            {
                _cyclic.SetEnabled(enabled);
            }
            else
            {
                _spring.SetEnabled(enabled);
            }
        }

        // UI-driven cyclic parameter updates
        public void SetCyclicSpringPercent(int percent)
        {
            _cyclic.SetSpringPercent(percent);
            if (_profile != null) _profile.CyclicSpring = Math.Clamp(percent, 0, 100);
        }

        public void SetCyclicDampingPercent(int percent)
        {
            _cyclic.SetDampingPercent(percent);
            if (_profile != null) _profile.CyclicDamping = Math.Clamp(percent, 0, 100);
        }

        // Read current stick position and set spring trim center accordingly
        public void SetTrimCenterFromCurrentStick(Joystick joystick)
        {
            if (joystick == null) return;
            try
            {
                var state = joystick.GetCurrentState();
                int x = state.X;
                int y = state.Y;
                if (_profile?.CyclicEnabled == true)
                {
                    _cyclic.SetCenterFromRaw(x, y);
                }
                else
                {
                    _spring.SetTrimCenterRaw(x, y);
                }
            }
            catch { }
        }

        public void Dispose()
        {
            ResetAll();
            _spring.Dispose();
            _shaker.Dispose();
            _gear.Dispose();
            _ground.Dispose();
            _cyclic.Dispose();
        }
    }
}
