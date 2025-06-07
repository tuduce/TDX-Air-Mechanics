using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.UI.Services;

/// <summary>
/// Main application service interface
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Start the application services
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop the application services
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Check if the application is running
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Check if SimConnect is connected
    /// </summary>
    bool IsSimConnectConnected { get; }

    /// <summary>
    /// Check if joystick is connected
    /// </summary>
    bool IsJoystickConnected { get; }

    /// <summary>
    /// Get current flight data
    /// </summary>
    FlightData? CurrentFlightData { get; }    /// <summary>
    /// Get current force feedback data
    /// </summary>
    ForceFeedbackData? CurrentForces { get; }

    /// <summary>
    /// Set the force feedback multiplier (0-100)
    /// </summary>
    /// <param name="multiplier">Force multiplier percentage</param>
    void SetForceMultiplier(int multiplier);

    /// <summary>
    /// Enable or disable force feedback
    /// </summary>
    /// <param name="enabled">Whether force feedback should be enabled</param>
    void SetForceFeedbackEnabled(bool enabled);

    /// <summary>
    /// Get available joystick devices
    /// </summary>
    /// <returns>List of available device names</returns>
    Task<List<string>> GetAvailableDevicesAsync();    /// <summary>
    /// Select a joystick device for force feedback
    /// </summary>
    /// <param name="deviceName">Name of the device to select</param>
    Task SelectDeviceAsync(string deviceName);

    /// <summary>
    /// Manually connect to the flight simulator
    /// </summary>
    /// <returns>True if connection was successful</returns>
    Task<bool> ConnectToSimulatorAsync();

    /// <summary>
    /// Manually disconnect from the flight simulator
    /// </summary>
    Task DisconnectFromSimulatorAsync();

    /// <summary>
    /// Event fired when the application status changes
    /// </summary>
    event EventHandler<ApplicationStatusEventArgs>? StatusChanged;
}

/// <summary>
/// Status service interface for UI updates
/// </summary>
public interface IStatusService
{
    /// <summary>
    /// Event fired when status changes
    /// </summary>
    event EventHandler<StatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Update connection status
    /// </summary>
    void UpdateConnectionStatus(bool isConnected, string details = "");

    /// <summary>
    /// Update force feedback status
    /// </summary>
    void UpdateForceFeedbackStatus(bool isActive, string details = "");

    /// <summary>
    /// Update general status message
    /// </summary>
    void UpdateStatus(string message, StatusLevel level = StatusLevel.Information);

    /// <summary>
    /// Get current status
    /// </summary>
    ApplicationStatus CurrentStatus { get; }
}

/// <summary>
/// Application status information
/// </summary>
public class ApplicationStatus
{
    /// <summary>
    /// Whether SimConnect is connected
    /// </summary>
    public bool IsSimConnectConnected { get; set; }

    /// <summary>
    /// SimConnect connection details
    /// </summary>
    public string SimConnectDetails { get; set; } = string.Empty;

    /// <summary>
    /// Whether force feedback is active
    /// </summary>
    public bool IsForceFeedbackActive { get; set; }

    /// <summary>
    /// Force feedback device details
    /// </summary>
    public string ForceFeedbackDetails { get; set; } = string.Empty;

    /// <summary>
    /// Current status message
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Status level
    /// </summary>
    public StatusLevel StatusLevel { get; set; } = StatusLevel.Information;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdate { get; set; } = DateTime.Now;
}

/// <summary>
/// Status level enumeration
/// </summary>
public enum StatusLevel
{
    Information,
    Warning,
    Error,
    Success
}

/// <summary>
/// Event arguments for application status changes
/// </summary>
public class ApplicationStatusEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public StatusLevel Level { get; set; } = StatusLevel.Information;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// Event arguments for status changes
/// </summary>
public class StatusChangedEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public StatusLevel Level { get; set; } = StatusLevel.Information;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public ApplicationStatus Status { get; set; } = new();
}
