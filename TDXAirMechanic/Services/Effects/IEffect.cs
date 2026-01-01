using SharpDX.DirectInput;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services.Effects
{
    public interface IEffect : IDisposable
    {
        void AttachDevice(Joystick joystick);
        void DetachDevice();
        void ApplyProfile(AirplaneProfile? profile);
        void Update(SimVariableData data);
        void Reset();
        void Start();
        void Stop();
    }
}
