using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services
{
    public class MechanicServices : IDisposable
    {
        private DirectInput _directInput;
        private DeviceInstance[] _joysticks;
        private CancellationTokenSource _cts;
        private Task? _mechanicTask;

        // This will be used to report data back to the UI thread safely
        private IProgress<MechanicProgress>? _progressReporter;

        // A flag to detect redundant calls to Dispose
        private bool _disposed = false;

        public MechanicServices()
        {
            _directInput = new DirectInput();
            _joysticks = Array.Empty<DeviceInstance>();
            _cts = new CancellationTokenSource();
        }

        public void Start(IProgress<MechanicProgress> progress)
        {
            if (_mechanicTask != null) return; // Already running

            _progressReporter = (IProgress<MechanicProgress>?)progress;
            _mechanicTask = Task.Run(() => DoMechanicWork());
        }

        private void DoMechanicWork()
        {
            LoadJoysticks();
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    // Simulate some mechanic work
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + "Error in mechanic work");
            }
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
