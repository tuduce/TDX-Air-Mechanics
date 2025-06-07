using Microsoft.Extensions.Logging;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.UI.Services;

/// <summary>
/// Implementation of the main application service
/// </summary>
public class ApplicationService : IApplicationService
{
    private readonly ILogger<ApplicationService> _logger;
    private readonly ISimConnectManager? _simConnectManager;
    private readonly IDirectInputManager? _directInputManager;
    private readonly IForceCalculationEngine _forceCalculationEngine;
    private readonly IConfigurationManager _configurationManager;
    private bool _isRunning;
    private FlightData? _currentFlightData;
    private ForceFeedbackData? _currentForces;
    private ForceConfiguration _forceConfig = new();
    private bool _forceFeedbackEnabled = true;
    private int _forceMultiplier = 100;

    public ApplicationService(
        ILogger<ApplicationService> logger,
        IForceCalculationEngine forceCalculationEngine,
        IConfigurationManager configurationManager,
        ISimConnectManager? simConnectManager = null,
        IDirectInputManager? directInputManager = null)
    {
        _logger = logger;
        _forceCalculationEngine = forceCalculationEngine;
        _configurationManager = configurationManager;
        _simConnectManager = simConnectManager;
        _directInputManager = directInputManager;

        // Subscribe to events if managers are available
        if (_simConnectManager != null)
        {
            _simConnectManager.FlightDataReceived += OnFlightDataReceived;
            _simConnectManager.ConnectionStatusChanged += OnSimConnectStatusChanged;
        }

        if (_directInputManager != null)
        {
            _directInputManager.JoystickConnectionChanged += OnJoystickConnectionChanged;
        }
    }

    public bool IsRunning => _isRunning;
    public bool IsSimConnectConnected => _simConnectManager?.IsConnected ?? false;
    public bool IsJoystickConnected => _directInputManager?.IsJoystickConnected ?? false;
    public FlightData? CurrentFlightData => _currentFlightData;
    public ForceFeedbackData? CurrentForces => _currentForces;

    public event EventHandler<ApplicationStatusEventArgs>? StatusChanged;    public async Task StartAsync()
    {
        _logger.LogInformation("Starting TDX Air Mechanics application services");
        
        try
        {
            // Load configuration
            var appConfig = await _configurationManager.LoadConfigurationAsync();
            _forceConfig = appConfig.ForceSettings ?? new ForceConfiguration();
            
            // Initialize SimConnect but don't auto-connect
            if (_simConnectManager != null)
            {
                // Only initialize without connecting - user can connect manually
                _logger.LogInformation("SimConnect manager available - waiting for manual connection");
                OnStatusChanged("SimConnect ready - use Connect button when simulator is running", StatusLevel.Information);
            }

            // Initialize DirectInput if available
            if (_directInputManager != null)
            {
                await _directInputManager.InitializeAsync();
            }

            _isRunning = true;
            
            OnStatusChanged("Application services started", StatusLevel.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start application services");
            OnStatusChanged($"Failed to start: {ex.Message}", StatusLevel.Error);
            throw;
        }
    }public async Task StopAsync()
    {
        _logger.LogInformation("Stopping TDX Air Mechanics application services");
        
        try
        {
            _isRunning = false;

            // Stop SimConnect if available
            if (_simConnectManager != null)
            {
                await _simConnectManager.StopAsync();
            }

            // Stop DirectInput if available
            if (_directInputManager != null)
            {
                await _directInputManager.StopAllEffectsAsync();
            }
            
            OnStatusChanged("Application services stopped", StatusLevel.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping application services");
            OnStatusChanged($"Error stopping: {ex.Message}", StatusLevel.Error);
        }
    }    private void OnFlightDataReceived(object? sender, FlightData flightData)
    {
        _currentFlightData = flightData;
        
        // Calculate forces if force feedback is enabled
        if (_forceFeedbackEnabled && _directInputManager?.IsJoystickConnected == true)
        {
            try
            {
                // Apply force multiplier to configuration
                var adjustedConfig = new ForceConfiguration
                {
                    GlobalMultiplier = _forceConfig.GlobalMultiplier * (_forceMultiplier / 100.0),
                    AerodynamicMultiplier = _forceConfig.AerodynamicMultiplier,
                    StallMultiplier = _forceConfig.StallMultiplier,
                    TurbulenceMultiplier = _forceConfig.TurbulenceMultiplier,
                    MaxForceLimit = _forceConfig.MaxForceLimit,
                    MinForceThreshold = _forceConfig.MinForceThreshold,
                    ApplySafetyLimits = _forceConfig.ApplySafetyLimits,
                    SmoothingFactor = _forceConfig.SmoothingFactor
                };

                // Calculate forces
                _currentForces = _forceCalculationEngine.CalculateForces(flightData, adjustedConfig);
                  // Apply forces to joystick
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _directInputManager.ApplyForceAsync(_currentForces);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to apply force feedback");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating forces");
            }
        }
    }    public void SetForceMultiplier(int multiplier)
    {
        _forceMultiplier = Math.Max(0, Math.Min(200, multiplier)); // Clamp between 0-200%
        _logger.LogInformation("Setting force multiplier to {Multiplier}%", _forceMultiplier);
    }

    public void SetForceFeedbackEnabled(bool enabled)
    {
        _forceFeedbackEnabled = enabled;
        _logger.LogInformation("Force feedback {Status}", enabled ? "enabled" : "disabled");
        
        if (!enabled && _directInputManager != null)
        {
            // Stop all effects when disabled
            Task.Run(async () => await _directInputManager.StopAllEffectsAsync());
        }
    }    public async Task<List<string>> GetAvailableDevicesAsync()
    {
        _logger.LogInformation("Getting available joystick devices");
        
        if (_directInputManager != null)
        {
            try
            {
                var devices = await _directInputManager.GetAvailableDevicesAsync();
                return devices.Select(d => d.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available devices");
                return new List<string> { "Error loading devices" };
            }
        }
        
        return new List<string> { "DirectInput manager not available" };
    }    public async Task SelectDeviceAsync(string deviceName)
    {
        await SelectDeviceAsync(deviceName, IntPtr.Zero);
    }

    public async Task SelectDeviceAsync(string deviceName, IntPtr windowHandle)
    {
        _logger.LogInformation("Selecting joystick device: {DeviceName}", deviceName);
        
        if (_directInputManager != null)
        {
            try
            {
                var devices = await _directInputManager.GetAvailableDevicesAsync();
                var device = devices.FirstOrDefault(d => d.Name == deviceName);
                if (device != null)
                {
                    await _directInputManager.SelectDeviceAsync(device.DeviceGuid, windowHandle);
                    OnStatusChanged($"Selected device: {deviceName}", StatusLevel.Success);
                }
                else
                {
                    OnStatusChanged($"Device not found: {deviceName}", StatusLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to select device");
                OnStatusChanged($"Failed to select device: {ex.Message}", StatusLevel.Error);
            }
        }
    }

    public async Task<bool> ConnectToSimulatorAsync()
    {
        _logger.LogInformation("Attempting manual connection to flight simulator");
        
        if (_simConnectManager == null)
        {
            _logger.LogWarning("SimConnect manager not available");
            OnStatusChanged("SimConnect manager not available", StatusLevel.Warning);
            return false;
        }

        try
        {
            var success = await _simConnectManager.ConnectAsync();
            if (success)
            {
                await _simConnectManager.StartAsync();
                OnStatusChanged("Connected to flight simulator", StatusLevel.Success);
                return true;
            }
            else
            {
                OnStatusChanged("Failed to connect to flight simulator", StatusLevel.Warning);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual SimConnect connection");
            OnStatusChanged($"Connection failed: {ex.Message}", StatusLevel.Error);
            return false;
        }
    }

    public async Task DisconnectFromSimulatorAsync()
    {
        _logger.LogInformation("Manually disconnecting from flight simulator");
        
        if (_simConnectManager == null)
        {
            _logger.LogWarning("SimConnect manager not available");
            return;
        }

        try
        {
            await _simConnectManager.DisconnectAsync();
            OnStatusChanged("Disconnected from flight simulator", StatusLevel.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual SimConnect disconnection");
            OnStatusChanged($"Disconnection failed: {ex.Message}", StatusLevel.Error);
        }
    }

    private void OnSimConnectStatusChanged(object? sender, bool isConnected)
    {
        var message = isConnected ? "SimConnect connected" : "SimConnect disconnected";
        OnStatusChanged(message, isConnected ? StatusLevel.Success : StatusLevel.Warning);
    }

    private void OnJoystickConnectionChanged(object? sender, bool isConnected)
    {
        var message = isConnected ? "Joystick connected" : "Joystick disconnected";
        OnStatusChanged(message, isConnected ? StatusLevel.Success : StatusLevel.Warning);
    }

    private void OnStatusChanged(string message, StatusLevel level)
    {
        _logger.LogInformation("Status changed: {Message}", message);
        StatusChanged?.Invoke(this, new ApplicationStatusEventArgs
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.Now
        });
    }

    public void Dispose()
    {
        if (_simConnectManager != null)
        {
            _simConnectManager.FlightDataReceived -= OnFlightDataReceived;
            _simConnectManager.ConnectionStatusChanged -= OnSimConnectStatusChanged;
        }

        if (_directInputManager != null)
        {
            _directInputManager.JoystickConnectionChanged -= OnJoystickConnectionChanged;
        }
    }
}

/// <summary>
/// Implementation of the status service
/// </summary>
public class StatusService : IStatusService
{
    private readonly ILogger<StatusService> _logger;
    private readonly ApplicationStatus _currentStatus = new();

    public StatusService(ILogger<StatusService> logger)
    {
        _logger = logger;
    }

    public ApplicationStatus CurrentStatus => _currentStatus;

    public event EventHandler<StatusChangedEventArgs>? StatusChanged;

    public void UpdateConnectionStatus(bool isConnected, string details = "")
    {
        _currentStatus.IsSimConnectConnected = isConnected;
        _currentStatus.SimConnectDetails = details;
        _currentStatus.LastUpdate = DateTime.Now;

        var message = isConnected ? "SimConnect connected" : "SimConnect disconnected";
        OnStatusChanged(message, isConnected ? StatusLevel.Success : StatusLevel.Warning);
    }

    public void UpdateForceFeedbackStatus(bool isActive, string details = "")
    {
        _currentStatus.IsForceFeedbackActive = isActive;
        _currentStatus.ForceFeedbackDetails = details;
        _currentStatus.LastUpdate = DateTime.Now;

        var message = isActive ? "Force feedback active" : "Force feedback inactive";
        OnStatusChanged(message, isActive ? StatusLevel.Success : StatusLevel.Warning);
    }

    public void UpdateStatus(string message, StatusLevel level = StatusLevel.Information)
    {
        _currentStatus.StatusMessage = message;
        _currentStatus.StatusLevel = level;
        _currentStatus.LastUpdate = DateTime.Now;

        OnStatusChanged(message, level);
    }

    private void OnStatusChanged(string message, StatusLevel level)
    {
        _logger.LogDebug("Status update: {Message} ({Level})", message, level);
        
        StatusChanged?.Invoke(this, new StatusChangedEventArgs
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.Now,
            Status = _currentStatus
        });
    }
}
