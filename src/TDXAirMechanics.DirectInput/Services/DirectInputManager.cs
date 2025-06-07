using Microsoft.Extensions.Logging;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.DirectInput.Services;

/// <summary>
/// DirectInput manager implementation for force feedback joystick control
/// </summary>
public class DirectInputManager : IDirectInputManager
{
    private readonly ILogger<DirectInputManager> _logger;
    private SharpDX.DirectInput.DirectInput? _directInput;
    private Joystick? _joystick;
    private bool _disposed;
    private JoystickInfo? _connectedJoystick;

    // DirectInput force range constants
    private const int DI_FORCE_MIN = -35767;
    private const int DI_FORCE_MAX = 35767;
    private const int DI_FORCE_CENTER = 0;
    private const float SAFETY_LIMIT = 0.85f; // 85% of max force for safety

    // Windows API import for desktop window handle
    [DllImport("user32.dll", SetLastError = false)]
    private static extern IntPtr GetDesktopWindow();

    public DirectInputManager(ILogger<DirectInputManager> logger)
    {
        _logger = logger;
    }

    public event EventHandler<bool>? JoystickConnectionChanged;

    public bool IsJoystickConnected => _joystick != null && _connectedJoystick != null;
    public JoystickInfo? ConnectedJoystick => _connectedJoystick;    public async Task<bool> InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing DirectInput for force feedback");
            
            await Task.Run(() =>
            {
                _directInput = new SharpDX.DirectInput.DirectInput();
            });
            
            // Find force-feedback capable joysticks but don't auto-select during initialization
            var joystickGuid = await FindForceFeedbackJoystickAsync();
            if (joystickGuid == Guid.Empty)
            {
                _logger.LogWarning("No force feedback joystick found");
                return false;
            }
            
            _logger.LogInformation("DirectInput initialized successfully - devices available for manual selection");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize DirectInput");
            return false;
        }
    }public async Task<bool> ApplyForceAsync(ForceFeedbackData forceData)
    {
        if (_joystick == null || !IsJoystickConnected)
        {
            return false;
        }

        try
        {
            await Task.Run(() =>
            {
                // Clamp forces to safety limits
                forceData.ClampForces();
                var safeForceX = forceData.ForceX * SAFETY_LIMIT;
                var safeForceY = forceData.ForceY * SAFETY_LIMIT;

                // Convert to DirectInput force values
                var diForceX = (int)(safeForceX * DI_FORCE_MAX);
                var diForceY = (int)(safeForceY * DI_FORCE_MAX);

                // TODO: Apply actual force effects using SharpDX
                // This is a placeholder implementation
                
                _logger.LogDebug("Applied force: X={ForceX:F3}, Y={ForceY:F3}", 
                    safeForceX, safeForceY);
            });
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply force feedback");
            return false;
        }
    }    public async Task StopAllEffectsAsync()
    {
        if (_joystick == null) return;

        try
        {
            await Task.Run(() =>
            {
                // TODO: Stop all force effects
                _logger.LogDebug("Stopped all force effects");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping force effects");
        }
    }    public async Task<List<JoystickInfo>> GetAvailableDevicesAsync()
    {
        var devices = new List<JoystickInfo>();
        
        if (_directInput == null) return devices;

        try
        {
            await Task.Run(() =>
            {
                foreach (var deviceInstance in _directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                {
                    var info = new JoystickInfo
                    {
                        DeviceGuid = deviceInstance.InstanceGuid,
                        Name = deviceInstance.InstanceName,
                        Manufacturer = deviceInstance.ProductName,
                        SupportsForceFeedback = (deviceInstance.Type & DeviceType.Joystick) != 0, // Simplified check
                        ProductId = deviceInstance.ProductGuid.GetHashCode(),
                        VendorId = deviceInstance.InstanceGuid.GetHashCode()
                    };
                    
                    devices.Add(info);
                }
                
                _logger.LogInformation("Found {Count} joystick devices", devices.Count);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating joystick devices");
        }

        return devices;
    }    public async Task<bool> SelectDeviceAsync(Guid deviceGuid, IntPtr windowHandle = default)
    {
        try
        {
            await Task.Run(() =>
            {
                // Dispose existing joystick
                _joystick?.Dispose();
                _joystick = null;
                _connectedJoystick = null;

                if (_directInput == null) return;

                _joystick = new Joystick(_directInput, deviceGuid);
                
                // Use the provided window handle, or fallback to desktop window handle
                var hwnd = windowHandle != IntPtr.Zero ? windowHandle : GetDesktopWindow();
                _joystick.SetCooperativeLevel(hwnd, 
                    CooperativeLevel.Background | CooperativeLevel.Exclusive);

                // Get device information
                var capabilities = _joystick.Capabilities;
                _connectedJoystick = new JoystickInfo
                {
                    DeviceGuid = deviceGuid,
                    Name = _joystick.Information.InstanceName,
                    Manufacturer = _joystick.Information.ProductName,
                    SupportsForceFeedback = capabilities.AxeCount > 0, // Simplified force feedback check
                    AxisCount = capabilities.AxeCount,
                    ButtonCount = capabilities.ButtonCount
                };

                _joystick.Acquire();
                
                _logger.LogInformation("Selected joystick: {Name} (Axes: {Axes})", 
                    _connectedJoystick.Name, _connectedJoystick.AxisCount);
            });
            
            JoystickConnectionChanged?.Invoke(this, true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to select joystick device");
            return false;
        }
    }    private async Task<Guid> FindForceFeedbackJoystickAsync()
    {
        var devices = await GetAvailableDevicesAsync();
        var ffDevice = devices.FirstOrDefault(d => d.SupportsForceFeedback);
        return ffDevice?.DeviceGuid ?? Guid.Empty;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                // Don't wait for async operations in Dispose - just clean up synchronously
                _joystick?.Dispose();
                _directInput?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during DirectInput disposal");
            }
            _disposed = true;
        }
    }
}
