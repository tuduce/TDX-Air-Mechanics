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
            if (_profile != null) ApplyProfile(_profile);
        }

        public void DetachDevice()
        {
            ResetAll();
            _spring.DetachDevice();
            _shaker.DetachDevice();
            _gear.DetachDevice();
            _ground.DetachDevice();
            _joystick = null;
        }

        public void ApplyProfile(AirplaneProfile? profile)
        {
            _profile = profile;
            _spring.ApplyProfile(profile);
            _shaker.ApplyProfile(profile);
            _gear.ApplyProfile(profile);
            _ground.ApplyProfile(profile);
        }

        public void Update(SimVariableData data)
        {
            if (GetJoystickSafe() == null || _profile == null) return;
            _spring.Update(data);
            _shaker.Update(data);
            _gear.Update(data);
            _ground.Update(data);
        }

        public void ResetAll()
        {
            _shaker.Reset();
            _gear.Reset();
            _ground.Reset();
            _spring.Reset();
        }

        public void NudgeTrim(int pitchDelta, int rollDelta)
        {
            _spring.NudgeTrim(pitchDelta, rollDelta);
        }

        public void Dispose()
        {
            ResetAll();
            _spring.Dispose();
            _shaker.Dispose();
            _gear.Dispose();
            _ground.Dispose();
        }
    }
}
