using Microsoft.Extensions.Logging;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.Core.Models;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using MSFSSimConnect = Microsoft.FlightSimulator.SimConnect.SimConnect;

namespace TDXAirMechanics.SimConnect.Services;

/// <summary>
/// SimConnect manager implementation for Microsoft Flight Simulator integration
/// </summary>
public class SimConnectManager : ISimConnectManager
{    private readonly ILogger<SimConnectManager> _logger;
    private bool _disposed;
    private bool _isConnected; private FlightData? _currentFlightData;
    private int _updateRateHz = 30;
    private System.Threading.Timer? _dataUpdateTimer;
    private System.Threading.Timer? _connectionMonitorTimer;

    private MSFSSimConnect? _simConnect;
    private readonly uint WM_USER_SIMCONNECT = 0x0402;
    private IntPtr _windowHandle = IntPtr.Zero;

    // SimConnect data definition IDs
    private enum DATA_DEFINITIONS
    {
        FlightDataDef,
    }

    // SimConnect request IDs  
    private enum DATA_REQUESTS
    {
        FlightDataRequest,
    }    // Data structure for SimConnect - fields must match exact order of AddToDataDefinition calls
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]    private struct SimFlightData
    {
        public double IndicatedAirspeed;    // AIRSPEED INDICATED
        public double Altitude;             // PLANE ALTITUDE
        public double VerticalSpeed;        // VERTICAL SPEED
        public double Heading;              // PLANE HEADING DEGREES TRUE
        public double Pitch;                // PLANE PITCH DEGREES
        public double Bank;                 // PLANE BANK DEGREES
        public double Throttle;             // GENERAL ENG THROTTLE LEVER POSITION:1
        public double EngineRPM;            // GENERAL ENG RPM:1
        public double FuelQuantity;         // FUEL TOTAL QUANTITY
        public double Flaps;                // FLAPS HANDLE PERCENT
        public double Gear;                 // GEAR HANDLE POSITION
        public double WindVelocity;         // AMBIENT WIND VELOCITY
        public double WindDirection;        // AMBIENT WIND DIRECTION
        public double GForce;               // G FORCE
        public double GroundSpeed;          // GROUND VELOCITY
        public double TrueAirspeed;         // AIRSPEED TRUE
        public double AngleOfAttack;        // INCIDENCE ALPHA
        public double SideSlipAngle;        // INCIDENCE BETA
    }

    // Windows API imports
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    public SimConnectManager(ILogger<SimConnectManager> logger)
    {
        _logger = logger;
    }

    public event EventHandler<FlightData>? FlightDataReceived;
    public event EventHandler<bool>? ConnectionStatusChanged;

    public bool IsConnected => _isConnected;
    public FlightData? CurrentFlightData => _currentFlightData;

    public int UpdateRateHz
    {
        get => _updateRateHz;
        set => _updateRateHz = Math.Max(1, Math.Min(60, value));
    }
    public async Task<bool> InitializeAsync()
    {
        return await ConnectAsync();
    }

    public async Task<bool> ConnectAsync()
    {        try
        {
            _logger.LogInformation("Attempting to connect to SimConnect");

            // Disconnect if already connected
            if (_simConnect != null)
            {
                await DisconnectAsync();
            }

            // Create a dummy window handle for SimConnect
            _windowHandle = GetConsoleWindow();
            if (_windowHandle == IntPtr.Zero)
            {
                _windowHandle = GetDesktopWindow();
            }            // Initialize SimConnect
            _simConnect = new MSFSSimConnect("TDXAirMechanics", _windowHandle, WM_USER_SIMCONNECT, null, 0);
            
            // Set up event handlers
            _simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
            _simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
            _simConnect.OnRecvException += SimConnect_OnRecvException;
            _simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;

            // Define data structures and requests
            DefineDataStructures();
            
            // Pump messages to trigger OnRecvOpen event
            var connectionTimeout = DateTime.UtcNow.AddSeconds(5);
            while (!_isConnected && DateTime.UtcNow < connectionTimeout)
            {
                try
                {
                    _simConnect.ReceiveMessage();
                }
                catch (COMException comEx) when (comEx.HResult == -2147023174) // RPC server unavailable
                {
                    _logger.LogWarning("Flight simulator is not running or SimConnect is not available");
                    return false;
                }
                await Task.Delay(50); // Small delay between message pumps
            }
            
            // Check if connection was successful
            if (!_isConnected)
            {
                _logger.LogWarning("SimConnect initialization completed but connection not established - simulator may not be running");
                return false;
            }

            _logger.LogInformation("SimConnect connected successfully");
            return true;
        }
        catch (COMException comEx) when (comEx.HResult == -2147023174) // 0x800706BA - RPC server unavailable
        {
            _logger.LogWarning("Flight simulator is not running or SimConnect is not available");
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, false);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to SimConnect");
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, false);
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            _logger.LogInformation("Disconnecting from SimConnect");            // Stop data collection
            await StopAsync();

            if (_simConnect != null)
            {
                try
                {
                    _simConnect.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing SimConnect instance");
                }
                finally
                {
                    _simConnect = null;
                }
            }

            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, false);
            _logger.LogInformation("SimConnect disconnected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SimConnect disconnect");
        }
    }    public async Task<bool> StartAsync()
    {        try
        {
            _logger.LogInformation("Starting SimConnect data collection");

            if (_simConnect != null && _isConnected)
            {
                // Set up continuous data request instead of periodic polling
                _simConnect.RequestDataOnSimObject(DATA_REQUESTS.FlightDataRequest, 
                    DATA_DEFINITIONS.FlightDataDef, 
                    MSFSSimConnect.SIMCONNECT_OBJECT_ID_USER, 
                    SIMCONNECT_PERIOD.SIM_FRAME, // Request data every sim frame
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 
                    0, 0, 0);
                
                // Start message pumping timer at higher frequency for responsive data reception
                var messagePumpIntervalMs = 1000 / 60; // 60Hz message pumping
                _dataUpdateTimer = new System.Threading.Timer(PumpMessages, null, 0, messagePumpIntervalMs);
                
                _logger.LogInformation("SimConnect continuous data request started with 60Hz message pumping");            }
            else
            {
                _logger.LogWarning("SimConnect not connected - cannot start data collection");
                return false;
            }

            await Task.Delay(100); // Give timer time to start
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start SimConnect");
            return false;
        }
    }
    public async Task StopAsync()
    {
        try
        {
            _logger.LogInformation("Stopping SimConnect data collection");            // Stop the timers
            _dataUpdateTimer?.Dispose();
            _dataUpdateTimer = null;
            _connectionMonitorTimer?.Dispose();
            _connectionMonitorTimer = null;

            // No additional cleanup needed for SimConnect data requests

            await Task.Delay(100); // Give timers time to stop
            _logger.LogInformation("SimConnect data collection stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping SimConnect");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {        if (!_disposed && disposing)
        {
            _dataUpdateTimer?.Dispose();
            _connectionMonitorTimer?.Dispose();

            try
            {
                _simConnect?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing SimConnect");
            }

            _disposed = true;
        }    }

    private void DefineDataStructures()
    {
        try
        {            // Define the data structure for flight data - ensure exact order match with struct
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "AIRSPEED INDICATED", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "VERTICAL SPEED", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "PLANE PITCH DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "PLANE BANK DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "GENERAL ENG THROTTLE LEVER POSITION:1", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "GENERAL ENG RPM:1", "rpm", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "FUEL TOTAL QUANTITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "FLAPS HANDLE PERCENT", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "GEAR HANDLE POSITION", "bool", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "AMBIENT WIND VELOCITY", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "AMBIENT WIND DIRECTION", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "G FORCE", "gforce", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "GROUND VELOCITY", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "AIRSPEED TRUE", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "INCIDENCE ALPHA", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "INCIDENCE BETA", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, MSFSSimConnect.SIMCONNECT_UNUSED);

            // Register the data definition
            _simConnect?.RegisterDataDefineStruct<SimFlightData>(DATA_DEFINITIONS.FlightDataDef);
            
            _logger.LogDebug("SimConnect data structures defined successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to define SimConnect data structures");
            throw;
        }
    }    private void SimConnect_OnRecvOpen(MSFSSimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
        _logger.LogInformation("SimConnect connection opened successfully - Application: {AppName}, Version: {Version}", 
            data.szApplicationName, $"{data.dwApplicationVersionMajor}.{data.dwApplicationVersionMinor}.{data.dwApplicationBuildMajor}.{data.dwApplicationBuildMinor}");
        _isConnected = true;
        ConnectionStatusChanged?.Invoke(this, true);
    }

    private void SimConnect_OnRecvQuit(MSFSSimConnect sender, SIMCONNECT_RECV data)
    {
        _logger.LogInformation("SimConnect connection closed by simulator");
        _isConnected = false;
        ConnectionStatusChanged?.Invoke(this, false);
    }

    private void SimConnect_OnRecvException(MSFSSimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
    {
        _logger.LogWarning("SimConnect exception: {Exception}", data.dwException);
    }

    private void SimConnect_OnRecvSimobjectData(MSFSSimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        try
        {            if (data.dwRequestID == (uint)DATA_REQUESTS.FlightDataRequest)
            {
                var simData = (SimFlightData)data.dwData[0];
                
                // Log all raw values to identify data mapping issues
                _logger.LogDebug("Raw SimConnect values: IAS={IAS:F2}, Alt={Alt:F2}, VS={VS:F2}, Hdg={Hdg:F2}, Pitch={Pitch:F2}, Bank={Bank:F2}", 
                    simData.IndicatedAirspeed, simData.Altitude, simData.VerticalSpeed, simData.Heading, simData.Pitch, simData.Bank);
                _logger.LogDebug("Raw SimConnect values: Throttle={Throttle:F2}, RPM={RPM:F2}, Fuel={Fuel:F2}, Flaps={Flaps:F2}, Gear={Gear:F2}", 
                    simData.Throttle, simData.EngineRPM, simData.FuelQuantity, simData.Flaps, simData.Gear);
                _logger.LogDebug("Raw SimConnect values: WindVel={WindVel:F2}, WindDir={WindDir:F2}, GForce={GForce:F2}, GroundSpd={GroundSpd:F2}, TrueAS={TrueAS:F2}", 
                    simData.WindVelocity, simData.WindDirection, simData.GForce, simData.GroundSpeed, simData.TrueAirspeed);
                
                _logger.LogDebug("Final data - Heading: {Heading:F1}°, Altitude: {Altitude:F0}ft, IAS: {IAS:F1}kts, TAS: {TAS:F1}kts, GS: {GS:F1}kts", 
                    simData.Heading, simData.Altitude, simData.IndicatedAirspeed, simData.TrueAirspeed, simData.GroundSpeed);
                  // Convert SimConnect data to our FlightData model
                var flightData = new FlightData
                {
                    Timestamp = DateTime.UtcNow,
                    AirspeedKnots = simData.IndicatedAirspeed,
                    AltitudeFeet = simData.Altitude,
                    VerticalSpeedFpm = simData.VerticalSpeed * 60, // Convert ft/s to ft/min
                    HeadingDegrees = simData.Heading,
                    PitchAngleDegrees = simData.Pitch,
                    BankAngleDegrees = simData.Bank,
                    ControlSurfaces = new ControlSurfaceData(),
                    Engine = new EngineData
                    {
                        ThrottlePosition = simData.Throttle / 100.0, // Convert from percentage
                        Rpm = simData.EngineRPM
                    },
                    Environment = new EnvironmentData
                    {
                        WindSpeedKnots = simData.WindVelocity, // Wind velocity in ft/s, convert to knots
                        WindDirectionDegrees = simData.WindDirection,
                        Turbulence = 0.0, // Not available in this basic implementation
                        TemperatureCelsius = 15.0, // Standard temp
                        BarometricPressure = 29.92 // Standard pressure
                    },
                    Aircraft = new AircraftConfiguration
                    {
                        Title = "Unknown Aircraft",
                        AircraftType = "General Aviation",
                        MaxWeightPounds = 2550,
                        CurrentWeightPounds = 2200,
                        WingSpanFeet = 36,
                        WingAreaSquareFeet = 174
                    },
                    OnGround = simData.Gear > 0.5, // Simplified ground detection
                    IsStalling = simData.AngleOfAttack > 15 // Simplified stall detection
                };

                _currentFlightData = flightData;
                FlightDataReceived?.Invoke(this, flightData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SimConnect data");
        }
    }    private void PumpMessages(object? state)
    {
        try
        {
            if (_simConnect != null && _isConnected)
            {
                // Process Windows messages to receive SimConnect data
                _simConnect.ReceiveMessage();
            }
        }
        catch (COMException comEx) when (comEx.HResult == -2147023174) // RPC server unavailable
        {
            _logger.LogWarning("SimConnect connection lost - simulator may have been closed");
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pumping SimConnect messages");
        }
    }
}

