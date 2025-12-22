using SharpDX.DirectInput;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services
{
    public interface IEffectsService : IDisposable
    {
        // Attach the current joystick device (already acquired by caller)
        void AttachDevice(Joystick joystick);

        // Detach and cleanup any effects associated with the current device
        void DetachDevice();

        // Apply or update the current airplane profile
        void ApplyProfile(AirplaneProfile? profile);

        // Update effects based on incoming simulator data
        void Update(SimVariableData data);

        // Stop and clear all effects on the current device
        void ResetAll();
    }
}
