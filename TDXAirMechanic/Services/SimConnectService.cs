using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services
{
    public class SimConnectService : IDisposable
    {
        private const int WM_USER_SIMCONNECT = 0x0402;

        private SimConnect? _simConnect;
        private CancellationTokenSource? _cts;
        private Task? _simConnectTask;

        // Thread-safe queue for commands from the UI thread
        private readonly ConcurrentQueue<SimCommand> _commandQueue = new();

        // This will be used to report data back to the UI thread safely
        private IProgress<AirplaneProfile>? _progressReporter;

        // A flag to detect redundant calls to Dispose
        private bool _disposed = false;

        // Define data requests and definitions
        private enum DEFINITIONS { Struct1 }
        private enum DATA_REQUESTS { Request1 }

        // This is the structure that will be sent to SimConnect
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi, Pack = 1)]
        struct SimResponse
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string title;
            public double indicated_altitude;
            public double indicated_airspeed;
        }

        public void Start(IProgress<AirplaneProfile> progress, IntPtr windowHandle)
        {
            if (_simConnectTask != null) return; // Already running

            _progressReporter = progress;
            _cts = new CancellationTokenSource();

            // Use Task.Run to start the SimConnect logic on a background thread
            _simConnectTask = Task.Run(() => ProcessSimConnectMessages(windowHandle, _cts.Token));

        }

        // This method is called by the UI thread to send a command
        public void EnqueueCommand(SimCommand command)
        {
            _commandQueue.Enqueue(command);
        }

        private void ProcessSimConnectMessages(IntPtr windowHandle, CancellationToken token)
        {
            try
            { 
                _simConnect = new SimConnect("TDX Air Mechanic", windowHandle, WM_USER_SIMCONNECT, null, 0);

                // --- Setup Event Handlers ---
                _simConnect.OnRecvOpen += OnRecvOpen;
                _simConnect.OnRecvQuit += OnRecvQuit;
                _simConnect.OnRecvException += OnRecvException;
                _simConnect.OnRecvSimobjectData += OnRecvSimobjectData;

                // --- Main Loop ---
                while (!token.IsCancellationRequested)
                {
                    // Process any commands from the UI thread
                    while (_commandQueue.TryDequeue(out var command))
                    {
                        ProcessCommand(command);
                    }

                    // Let SimConnect process its messages
                    _simConnect?.ReceiveMessage();

                    // Small delay to prevent a tight loop from consuming 100% CPU
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, perhaps report them back to the UI
                Debug.WriteLine($"SimConnect thread exception: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        private void ProcessCommand(SimCommand command)
        {
            if (_simConnect == null) return;

            switch (command.CommandType)
            {
                case SimCommandType.SetAutopilotHeading:
                    // Example: Transmit a client event to the sim
                    // _simConnect.TransmitClientEvent(...);
                    Debug.WriteLine($"Command received: Set heading to {command.Value}");
                    break;

                case SimCommandType.ToggleParkingBrake:
                    // ...
                    break;
            }
        }

        private void OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Debug.WriteLine("SimConnect connection opened.");

            // --- Register Data Definitions ---
            _simConnect.AddToDataDefinition(DEFINITIONS.Struct1, "TITLE", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(DEFINITIONS.Struct1, "INDICATED ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect.AddToDataDefinition(DEFINITIONS.Struct1, "AIRSPEED INDICATED", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

            _simConnect.RegisterDataDefineStruct<SimResponse>(DEFINITIONS.Struct1);

            // Request data every second
            _simConnect.RequestDataOnSimObject(DATA_REQUESTS.Request1, DEFINITIONS.Struct1, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SECOND, 0, 0, 0, 0);
        }

        private void OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (data.dwRequestID == (uint)DATA_REQUESTS.Request1)
            {
                var simResponse = (SimResponse)data.dwData[0];

                // Create a data object to send to the UI
                var uiData = new AirplaneProfile
                { 
                    Model = simResponse.title 
                };

                // Report progress, which will safely update the UI on the UI thread
                _progressReporter?.Report(uiData);
            }
        }

        // Other event handlers
        private void OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Debug.WriteLine("SimConnect has quit.");
            Dispose();
        }

        private void OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Debug.WriteLine($"SimConnect Exception: {data.dwException}");
        }

        private void Disconnect()
        {
            if (_simConnect != null)
            {
                // Send a disconnect message
                // Create a data object to send to the UI
                var uiData = new AirplaneProfile
                {
                    Model = "Click paper plane for MSFS"
                };

                _progressReporter?.Report(uiData);

                _simConnect.Dispose();
                _simConnect = null;
                Debug.WriteLine("SimConnect Disconnected.");
            }
        }

        // Public dispose method (the one consumers will call)
        public void Dispose()
        {
            // Dispose of managed and unmanaged resources.
            Dispose(true);
            // Suppress finalization to prevent the finalizer from running.
            GC.SuppressFinalize(this);
        }

        // Protected virtual dispose method
        protected virtual void Dispose(bool disposing)
        {
            // Check if Dispose has already been called.
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // --- This is the key part ---
                // 1. Signal the background task to stop
                _cts?.Cancel();

                // 2. Wait for the task to complete its current loop and exit gracefully
                try
                {
                    _simConnectTask?.Wait();
                }
                catch (AggregateException ex)
                {
                    // Handle exceptions that might occur when waiting for the task
                    ex.Handle(e => e is OperationCanceledException);
                }

                // 3. Dispose of managed resources
                _cts?.Dispose();
                _simConnectTask?.Dispose();
            }

            // 4. Disconnect and clean up unmanaged SimConnect resources
            Disconnect();

            // 5. Mark that this object has been disposed.
            _disposed = true;
        }


        // A helper class to create a message-only window for SimConnect
        private class MessageWindow : NativeWindow, IDisposable
        {
            public void Dispose() => DestroyHandle();
        }
    }
}
