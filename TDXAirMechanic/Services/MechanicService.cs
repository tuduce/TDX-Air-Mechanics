using SharpDX.DirectInput;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services
{
    public class MechanicService : IDisposable
    {
        private readonly DirectInput _directInput;
        private DeviceInstance[] _joysticks;
        private readonly CancellationTokenSource _cts;
        private Task? _mechanicTask;

        // Channel to receive simulator variables from SimConnectService (thread-safe)
        private readonly Channel<SimVariableData> _simDataChannel = Channel.CreateUnbounded<SimVariableData>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = true }
        );

        // This will be used to report data back to the UI thread safely
        private IProgress<MechanicProgress>? _progressReporter;

        // A flag to detect redundant calls to Dispose
        private bool _disposed = false;

        // Currently active joystick device and instance
        private Joystick? _activeJoystick;
        private DeviceInstance? _activeJoystickDevice;

        // Effects manager
        private readonly IEffectsService _effects;

        // Current profile to read trim button bindings
        private AirplaneProfile? _activeProfile;

        // Track currently pressed trim buttons and last repeat time
        private readonly Dictionary<int, long> _pressedTrimButtons = new(); // buttonIndex -> lastTick (Environment.TickCount64)

        // Joystick button capture for UI: when set, the next button press reports its index
        private Action<int>? _buttonCaptureCallback;
        private readonly HashSet<int> _pressedCaptureButtons = new();

        // Flight loaded state gates whether effects are active
        private volatile bool _effectsEnabled = false;

        public MechanicService(IEffectsService effects)
        {
            _directInput = new();
            _joysticks = [];
            _cts = new();
            _effects = effects;
        }

        public void Start(IProgress<MechanicProgress> progress)
        {
            if (_mechanicTask != null) return; // Already running

            _progressReporter = (IProgress<MechanicProgress>?)progress;
            _mechanicTask = Task.Run(DoMechanicWorkAsync);
        }

        // Called by UI to reflect SimStart/SimStop
        public void SetFlightLoaded(bool loaded)
        {
            _effectsEnabled = loaded;

            if (!loaded)
            {
                // Stop and clear all effects immediately
                _effects.ResetAll();
            }
            else
            {
                // Re-apply current profile if any to create necessary effects
                if (_activeProfile != null)
                {
                    _effects.ApplyProfile(_activeProfile);
                }
            }
        }

        // UI can begin capture of next joystick button press
        public void BeginButtonCapture(Action<int> onCaptured)
        {
            _buttonCaptureCallback = onCaptured;
            _pressedCaptureButtons.Clear();
        }

        public void CancelButtonCapture()
        {
            _buttonCaptureCallback = null;
            _pressedCaptureButtons.Clear();
        }

        // Called by UI to update the active profile or reflect changes
        public void SetActiveProfile(AirplaneProfile profile)
        {
            _activeProfile = profile;
            if (_effectsEnabled)
            {
                _effects.ApplyProfile(profile);
            }
            Debug.WriteLine($"[Mechanic] Active profile set: Model={profile.Model}, Centered={profile.CenteredSpring}, Dynamic={profile.DynamicSpring}, Shaker={profile.StickShaker}, GearVibration={profile.GearVibration}");
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
            if (device == null || device.InstanceGuid == Guid.Empty)
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
            _effects.DetachDevice();

            var sb = new StringBuilder();

            // Create and configure
            var joystick = new Joystick(_directInput, device.InstanceGuid);

            // Set cooperative level - use Background to keep FFB active when app loses focus
            // Many drivers require Exclusive for FFB, but Foreground causes effects to stop on focus loss.
            joystick.SetCooperativeLevel(hwnd, CooperativeLevel.Exclusive | CooperativeLevel.Background);

            // Recommended defaults
            try { joystick.Properties.BufferSize = 128; } catch { }
            try { joystick.Properties.AutoCenter = false; } catch { }
            try { joystick.Properties.ForceFeedbackGain = 10000; } catch { }

            // Acquire and reset/stop any previous FFB
            joystick.Acquire();
            try { joystick.SendForceFeedbackCommand(ForceFeedbackCommand.StopAll); } catch { }
            try { joystick.SendForceFeedbackCommand(ForceFeedbackCommand.Reset); } catch { }

            // Attach to effects manager and apply current profile only if effects enabled
            _effects.AttachDevice(joystick);
            if (_effectsEnabled && _activeProfile != null)
            {
                _effects.ApplyProfile(_activeProfile);
            }

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
                sb.AppendLine($"Cooperative: Exclusive|Background");
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
            try
            {
                var reader = _simDataChannel.Reader;
                while (!_cts.IsCancellationRequested)
                {
                    // Drain queued items quickly
                    while (reader.TryRead(out var queued))
                    {
                        ProcessSimData(queued);
                    }

                    // Poll trim buttons ~50Hz even when no sim data is incoming
                    PollTrimButtons();

                    // Wait either for new sim data or a short delay for next poll
                    var waitTask = reader.WaitToReadAsync(_cts.Token).AsTask();
                    var delayTask = Task.Delay(20, _cts.Token);
                    var completed = await Task.WhenAny(waitTask, delayTask);
                    if (completed == waitTask && waitTask.Result)
                    {
                        // There's data available; process one to keep loop responsive
                        if (reader.TryRead(out var next))
                            ProcessSimData(next);
                    }
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

        private void PollTrimButtons()
        {
            if (!_effectsEnabled) return; // no flight loaded -> ignore inputs

            var js = _activeJoystick;
            var profile = _activeProfile;
            if (js == null || profile == null) return;
            if (js.NativePointer == IntPtr.Zero) return;

            try
            {
                var state = js.GetCurrentState();
                var buttons = state.Buttons;
                if (buttons == null || buttons.Length == 0) return;

                // Handle UI capture first: capture next rising edge and then clear callback
                if (_buttonCaptureCallback != null)
                {
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        bool down = buttons[i];
                        if (down)
                        {
                            if (_pressedCaptureButtons.Add(i))
                            {
                                var cb = _buttonCaptureCallback;
                                _buttonCaptureCallback = null; // only once
                                _pressedCaptureButtons.Clear();
                                cb?.Invoke(i);
                                break;
                            }
                        }
                        else
                        {
                            _pressedCaptureButtons.Remove(i);
                        }
                    }
                }

                // Trim buttons only when enabled
                if (!profile.TrimEnabled) return;

                int trimStep = profile.TrimStep;
                long now = Environment.TickCount64;

                void HandleButton(int index, Action onPress)
                {
                    if (index < 0 || index >= buttons.Length) return;
                    bool isDown = buttons[index];
                    if (isDown)
                    {
                        if (!_pressedTrimButtons.TryGetValue(index, out var lastTick))
                        {
                            _pressedTrimButtons[index] = now; // record first press
                            onPress();
                        }
                        else if (now - lastTick >= 100)
                        {
                            _pressedTrimButtons[index] = now; // repeat every 100ms
                            onPress();
                        }
                    }
                    else
                    {
                        if (_pressedTrimButtons.ContainsKey(index))
                            _pressedTrimButtons.Remove(index);
                    }
                }

                HandleButton(profile.RollTrimLeftButton, () => _effects.NudgeTrim(0, -trimStep));
                HandleButton(profile.RollTrimRightButton, () => _effects.NudgeTrim(0, trimStep));
                HandleButton(profile.PitchTrimUpButton, () => _effects.NudgeTrim(trimStep, 0));
                HandleButton(profile.PitchTrimDownButton, () => _effects.NudgeTrim(-trimStep, 0));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "[Mechanic] Failed to poll trim buttons");
            }
        }

        private void ProcessSimData(SimVariableData data)
        {
            if (!_effectsEnabled) return; // ignore sim data when no flight loaded
            // Forward to effects manager. It decides what to do based on current profile
            _effects.Update(data);
        }

        public void LoadJoysticks()
        {
            // TODO: settle the initial loading of the joysticks
            // The window must be foreground to acquire the joystick

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
                    _progress.Command = MechanicProgressCommand.SetStatus;
                    _progress.Status = "No force feedback devices found.";
                    _progressReporter?.Report(_progress);
                    return;
                }

                // Add joystick names to dropdown
                foreach (var joystick in _joysticks)
                {
                    _progress.Joysticks.Add(joystick.InstanceName);
                }
                _progress.Command = MechanicProgressCommand.SetJoysticks;
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
                using var tempJoystick = new Joystick(_directInput, device.InstanceGuid);
                return tempJoystick.Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback);
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

                // Reset and dispose effects
                _effects.ResetAll();
                (_effects as IDisposable)?.Dispose();

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
