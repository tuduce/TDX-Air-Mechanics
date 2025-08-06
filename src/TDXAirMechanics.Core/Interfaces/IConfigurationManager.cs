using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.Core.Interfaces;

/// <summary>
/// Interface for configuration management
/// </summary>
public interface IConfigurationManager
{
    /// <summary>
    /// Load application configuration
    /// </summary>
    /// <returns>Application configuration</returns>
    Task<AppConfiguration> LoadConfigurationAsync();

    /// <summary>
    /// Save application configuration
    /// </summary>
    /// <param name="config">Configuration to save</param>
    Task SaveConfigurationAsync(AppConfiguration config);

    /// <summary>
    /// Load aircraft profiles
    /// </summary>
    /// <returns>List of aircraft profiles</returns>
    Task<List<AircraftForceProfile>> LoadAircraftProfilesAsync();

    /// <summary>
    /// Save aircraft profiles
    /// </summary>
    /// <param name="profiles">Profiles to save</param>
    Task SaveAircraftProfilesAsync(List<AircraftForceProfile> profiles);

    /// <summary>
    /// Get profile for specific aircraft
    /// </summary>
    /// <param name="aircraftTitle">Aircraft title/identifier</param>
    /// <returns>Aircraft profile or null if not found</returns>
    Task<AircraftForceProfile?> GetAircraftProfileAsync(string aircraftTitle);

    /// <summary>
    /// Create default configuration
    /// </summary>
    /// <returns>Default configuration</returns>
    AppConfiguration CreateDefaultConfiguration();
}

/// <summary>
/// Application configuration
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// General application settings
    /// </summary>
    public GeneralSettings General { get; set; } = new();

    /// <summary>
    /// SimConnect configuration
    /// </summary>
    public SimConnectSettings SimConnect { get; set; } = new();

    /// <summary>
    /// DirectInput configuration
    /// </summary>
    public DirectInputSettings DirectInput { get; set; } = new();

    /// <summary>
    /// Force feedback configuration
    /// </summary>
    public ForceConfiguration ForceSettings { get; set; } = new();

    /// <summary>
    /// UI settings
    /// </summary>
    public UISettings UI { get; set; } = new();
}

/// <summary>
/// General application settings
/// </summary>
public class GeneralSettings
{
    /// <summary>
    /// Whether to start with Windows
    /// </summary>
    public bool StartWithWindows { get; set; }

    /// <summary>
    /// Whether to close application immediately instead of minimizing to tray
    /// </summary>
    public bool CloseToTrayOnExit { get; set; } = true;

    /// <summary>
    /// Auto-connect to MSFS when available
    /// </summary>
    public bool AutoConnect { get; set; } = true;

    /// <summary>
    /// Application theme
    /// </summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>
    /// Logging level
    /// </summary>
    public string LogLevel { get; set; } = "Information";
}

/// <summary>
/// SimConnect specific settings
/// </summary>
public class SimConnectSettings
{
    /// <summary>
    /// Data update rate in Hz
    /// </summary>
    public int UpdateRateHz { get; set; } = 30;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Auto-reconnect on connection loss
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Reconnection delay in seconds
    /// </summary>
    public int ReconnectDelaySeconds { get; set; } = 5;
}

/// <summary>
/// DirectInput specific settings
/// </summary>
public class DirectInputSettings
{
    /// <summary>
    /// Preferred joystick device GUID
    /// </summary>
    public string? PreferredDeviceGuid { get; set; }

    /// <summary>
    /// Auto-select first available device
    /// </summary>
    public bool AutoSelectDevice { get; set; } = true;

    /// <summary>
    /// Force effect update rate in Hz
    /// </summary>
    public int EffectUpdateRateHz { get; set; } = 60;

    /// <summary>
    /// Enable device monitoring for hot-plug support
    /// </summary>
    public bool EnableDeviceMonitoring { get; set; } = true;
}

/// <summary>
/// User interface settings
/// </summary>
public class UISettings
{
    /// <summary>
    /// Main window size and position
    /// </summary>
    public WindowSettings MainWindow { get; set; } = new();

    /// <summary>
    /// Whether to show advanced options
    /// </summary>
    public bool ShowAdvancedOptions { get; set; }

    /// <summary>
    /// Refresh rate for UI updates in Hz
    /// </summary>
    public int RefreshRateHz { get; set; } = 10;
}

/// <summary>
/// Window settings
/// </summary>
public class WindowSettings
{
    /// <summary>
    /// Window X position
    /// </summary>
    public int X { get; set; } = 100;

    /// <summary>
    /// Window Y position
    /// </summary>
    public int Y { get; set; } = 100;

    /// <summary>
    /// Window width
    /// </summary>
    public int Width { get; set; } = 800;

    /// <summary>
    /// Window height
    /// </summary>
    public int Height { get; set; } = 600;

    /// <summary>
    /// Whether window is maximized
    /// </summary>
    public bool IsMaximized { get; set; }
}
