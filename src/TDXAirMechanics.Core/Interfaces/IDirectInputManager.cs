using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.Core.Interfaces;

/// <summary>
/// Interface for DirectInput force feedback management
/// </summary>
public interface IDirectInputManager : IDisposable
{
    /// <summary>
    /// Event fired when joystick connection status changes
    /// </summary>
    event EventHandler<bool>? JoystickConnectionChanged;

    /// <summary>
    /// Initialize DirectInput and find force feedback devices
    /// </summary>
    /// <returns>True if initialization was successful</returns>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Apply force feedback to the joystick
    /// </summary>
    /// <param name="forceData">Force data to apply</param>
    /// <returns>True if force was applied successfully</returns>
    Task<bool> ApplyForceAsync(ForceFeedbackData forceData);

    /// <summary>
    /// Apply a generic force feedback effect to the joystick
    /// </summary>
    /// <param name="effect">Effect to apply</param>
    /// <returns>True if effect was applied successfully</returns>
    Task<bool> ApplyEffectAsync(ForceFeedbackEffect effect);

    /// <summary>
    /// Stop all force effects
    /// </summary>
    Task StopAllEffectsAsync();

    /// <summary>
    /// Check if a force feedback joystick is connected
    /// </summary>
    bool IsJoystickConnected { get; }

    /// <summary>
    /// Get information about the connected joystick
    /// </summary>
    JoystickInfo? ConnectedJoystick { get; }

    /// <summary>
    /// List all available force feedback devices
    /// </summary>
    /// <returns>List of available devices</returns>
    Task<List<JoystickInfo>> GetAvailableDevicesAsync();    /// <summary>
    /// Select a specific joystick by GUID
    /// </summary>
    /// <param name="deviceGuid">Device GUID</param>
    /// <param name="windowHandle">Window handle for cooperative level (optional, uses desktop if null)</param>
    /// <returns>True if selection was successful</returns>
    Task<bool> SelectDeviceAsync(Guid deviceGuid, IntPtr windowHandle = default);
}

/// <summary>
/// Information about a joystick device
/// </summary>
public class JoystickInfo
{
    /// <summary>
    /// Device GUID
    /// </summary>
    public Guid DeviceGuid { get; set; }

    /// <summary>
    /// Device name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the device supports force feedback
    /// </summary>
    public bool SupportsForceFeedback { get; set; }

    /// <summary>
    /// Number of axes
    /// </summary>
    public int AxisCount { get; set; }

    /// <summary>
    /// Number of buttons
    /// </summary>
    public int ButtonCount { get; set; }

    /// <summary>
    /// Device manufacturer
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// Device product ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Device vendor ID
    /// </summary>
    public int VendorId { get; set; }
}
