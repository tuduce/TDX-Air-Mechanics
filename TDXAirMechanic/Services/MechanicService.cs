using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services
{
    public class MechanicService : IDisposable
    {
        private DirectInput _directInput;
        private DeviceInstance[] _joysticks;
        private CancellationTokenSource _cts;
        private Task? _mechanicTask;

        // Channel to receive simulator variables from SimConnectService (thread-safe)
        private readonly Channel<SimVariableData> _simDataChannel = Channel.CreateUnbounded<SimVariableData>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = true }
        );

        // This will be used to report data back to the UI thread safely
        private IProgress<MechanicProgress>? _progressReporter;

        // A flag to detect redundant calls to Dispose
        private bool _disposed = false;

        // The currently active airplane profile (set by the UI)
        private AirplaneProfile? _activeProfile;

        // Currently active joystick device and instance
        private Joystick? _activeJoystick;
        private DeviceInstance? _activeJoystickDevice;

        public MechanicService()
        {
            _directInput = new DirectInput();
            _joysticks = Array.Empty<DeviceInstance>();
            _cts = new CancellationTokenSource();
        }

        public void Start(IProgress<MechanicProgress> progress)
        {
            if (_mechanicTask != null) return; // Already running

            _progressReporter = (IProgress<MechanicProgress>?)progress;
            _mechanicTask = Task.Run(DoMechanicWorkAsync);
        }

        // Called by UI to update the active profile or reflect changes
        public void SetActiveProfile(AirplaneProfile profile)
        {
            _activeProfile = profile;
            Debug.WriteLine($"[Mechanic] Active profile set: Model={profile.Model}, Centered={profile.CenteredSpring}, Dynamic={profile.DynamicSpring}, Shaker={profile.StickShaker}");
        }

        // Called by UI when the selected joystick changes
        // hwnd is the window handle used for setting cooperative level
        public string SelectJoystick(string? name, IntPtr hwnd)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "No joystick selected";
            }

            if (_joysticks == null || _joysticks.Length == 0)
            {
                return "No force feedback devices found";
            }

            var device = _joysticks.FirstOrDefault(j => string.Equals(j.InstanceName, name, StringComparison.Ordinal));
            if (device.InstanceGuid == Guid.Empty)
            {
                return $"Device '{name}' not found";
            }

            try
            {
                InitializeJoystick(device, hwnd, out var infoText);
                _activeJoystickDevice = device;
                return infoText;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "Error selecting joystick");
                return $"Failed to select device '{name}': {ex.Message}";
            }
        }

        private static string CleanEffectName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            return name.Replace("GUID_", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private void InitializeJoystick(DeviceInstance device, IntPtr hwnd, out string infoText)
        {
            // Cleanup previous
            try { _activeJoystick?.Unacquire(); } catch { }
            _activeJoystick?.Dispose();
            _activeJoystick = null;

            var sb = new StringBuilder();

            // Create and configure
            var joystick = new Joystick(_directInput, device.InstanceGuid);

            // Set cooperative level - Exclusive|Foreground is commonly required for FFB
            joystick.SetCooperativeLevel(hwnd, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);

            // Recommended defaults
            try { joystick.Properties.BufferSize = 128; } catch { }
            try { joystick.Properties.AutoCenter = false; } catch { }
            try { joystick.Properties.ForceFeedbackGain = 10000; } catch { }

            // Acquire and reset/stop any previous FFB
            joystick.Acquire();
            try { joystick.SendForceFeedbackCommand(ForceFeedbackCommand.StopAll); } catch { }
            try { joystick.SendForceFeedbackCommand(ForceFeedbackCommand.Reset); } catch { }

            // Enumerate supported effects
            var effects = new List<string>();
            try
            {
                foreach (var eff in joystick.GetEffects())
                {
                    effects.Add(CleanEffectName(eff.Name));
                }
            }
            catch { }

            // Build info
            sb.AppendLine($"Name: {device.InstanceName}");
            sb.AppendLine($"Product: {device.ProductName}");
            sb.AppendLine($"GUID: {device.InstanceGuid}");
            try
            {
                var caps = joystick.Capabilities;
                sb.AppendLine($"Axes: {caps.AxeCount}, Buttons: {caps.ButtonCount}, POVs: {caps.PovCount}");
                sb.AppendLine($"FFB: {(caps.Flags.HasFlag(DeviceFlags.ForceFeedback) ? "Yes" : "No")}");
            }
            catch { }
            if (effects.Count > 0)
            {
                sb.AppendLine("Effects:");
                foreach (var name in effects.Distinct())
                    sb.AppendLine(" - " + name);
            }

            _activeJoystick = joystick;
            infoText = sb.ToString();
        }

        // Method called by SimConnectService to enqueue sim data (non-blocking)
        public bool TryEnqueueSimData(SimVariableData data) => _simDataChannel.Writer.TryWrite(data);

        // Method called by SimConnectService to enqueue sim data (async)
        public ValueTask EnqueueSimDataAsync(SimVariableData data, CancellationToken ct = default) => _simDataChannel.Writer.WriteAsync(data, ct);

        private async Task DoMechanicWorkAsync()
        {
            LoadJoysticks();
            try
            {
                var reader = _simDataChannel.Reader;
                while (!_cts.IsCancellationRequested)
                {
                    // Prefer draining any queued items quickly
                    while (reader.TryRead(out var queued))
                    {
                        ProcessSimData(queued);
                    }

                    // Await next item or cancellation
                    var next = await reader.ReadAsync(_cts.Token);
                    ProcessSimData(next);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "Error in mechanic work");
            }
        }

        private void ProcessSimData(SimVariableData data)
        {
            // TODO: Implement actual mechanic logic reacting to sim variables and current profile
            var profileInfo = _activeProfile is null ? "(no profile)" : $"Centered={_activeProfile.CenteredSpring}, Dynamic={_activeProfile.DynamicSpring}, Shaker={_activeProfile.StickShaker}";
            // Debug.WriteLine($"[Mechanic] Model={data.Title}, Alt={data.IndicatedAltitude}, IAS={data.IndicatedAirspeed}, Profile={profileInfo}");
        }

        public void LoadJoysticks()
        {
            MechanicProgress _progress = new();
            try
            {
                // Get all joystick devices
                if (_directInput != null)
                {
                    _joysticks = _directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly)
                        .Where(device => IsDeviceForceFeedbackEnabled(device))
                        .ToArray();
                }

                if (_joysticks == null || _joysticks.Length == 0)
                {
                    _progress.Status = "No force feedback devices found.";
                    _progressReporter?.Report(_progress);
                    return;
                }

                // Add joystick names to dropdown
                foreach (var joystick in _joysticks)
                {
                    _progress.Joysticks.Add(joystick.InstanceName);
                }
                _progress.Status = _progress.Joysticks.FirstOrDefault();
                _progressReporter?.Report(_progress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "Error loading joysticks");
            }
        }

        private bool IsDeviceForceFeedbackEnabled(DeviceInstance device)
        {
            if (_directInput == null)
                return false;

            try
            {
                using (var tempJoystick = new Joystick(_directInput, device.InstanceGuid))
                {
                    return tempJoystick.Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + $"Error checking force feedback for device: {device.InstanceName}");
                return false;
            }
        }

        public void Dispose()
        {
            // Dispose of managed and unmanaged resources.
            Dispose(true);
            // Suppress finalization to prevent the finalizer from running.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check if Dispose has already been called.
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // 1. Signal the background task to stop
                _cts?.Cancel();

                // 2. Wait for the task to complete
                try
                {
                    _mechanicTask?.Wait();
                }
                catch (AggregateException ex)
                {
                    // Handle exceptions that might occur when waiting for the task
                    ex.Handle(e => e is OperationCanceledException);
                }

                // Unacquire and dispose the active joystick
                try { _activeJoystick?.Unacquire(); } catch { }
                _activeJoystick?.Dispose();
                _activeJoystick = null;
                _activeJoystickDevice = null;

                // 3. Dispose of managed resources
                _cts?.Dispose();
                _directInput?.Dispose();
                _mechanicTask?.Dispose();
            }

            // 4. Mark that this object has been disposed.
            _disposed = true;
        }

    }
}
