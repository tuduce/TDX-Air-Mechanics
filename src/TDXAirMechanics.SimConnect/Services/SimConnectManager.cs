using Microsoft.Extensions.Logging;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.Core.Models;

#if SIMCONNECT_AVAILABLE
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
#endif

namespace TDXAirMechanics.SimConnect.Services;

/// <summary>
/// SimConnect manager implementation for Microsoft Flight Simulator integration
/// </summary>
public class SimConnectManager : ISimConnectManager
{
    private readonly ILogger<SimConnectManager> _logger;
    private bool _disposed;
    private bool _isConnected;
    private FlightData? _currentFlightData;
    private int _updateRateHz = 30;
    private System.Threading.Timer? _dataUpdateTimer;

#if SIMCONNECT_AVAILABLE
    private Microsoft.FlightSimulator.SimConnect.SimConnect? _simConnect;
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
    }

    // Data structure for SimConnect
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    private struct SimFlightData
    {
        public double Airspeed;
        public double Altitude;
        public double VerticalSpeed;
        public double Heading;
        public double Pitch;
        public double Bank;
        public double Throttle;
        public double EngineRPM;
        public double FuelQuantity;
        public double Flaps;
        public double Gear;
        public double WindVelocityX;
        public double WindVelocityY;
        public double WindVelocityZ;
        public double GForce;
        public double GroundSpeed;
        public double TrueAirspeed;
        public double IndicatedAirspeed;
        public double AngleOfAttack;
        public double SideSlipAngle;
    }

    // Windows API imports
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();
#endif

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
        try
        {
            _logger.LogInformation("Initializing SimConnect connection");
            
#if SIMCONNECT_AVAILABLE
            // Create a dummy window handle for SimConnect
            _windowHandle = GetConsoleWindow();
            if (_windowHandle == IntPtr.Zero)
            {
                _windowHandle = GetDesktopWindow();
            }

            // Initialize SimConnect
            _simConnect = new Microsoft.FlightSimulator.SimConnect.SimConnect("TDXAirMechanics", _windowHandle, WM_USER_SIMCONNECT, null, 0);
            
            // Set up event handlers
            _simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
            _simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
            _simConnect.OnRecvException += SimConnect_OnRecvException;
            _simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;

            // Define data structures and requests
            DefineDataStructures();
            
            await Task.Delay(100); // Give SimConnect time to initialize
#else
            // Fallback when SimConnect SDK is not available
            await Task.Delay(100);
            _logger.LogWarning("SimConnect SDK not available - running in simulation mode");
#endif
            
            _logger.LogInformation("SimConnect initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SimConnect");
            return false;
        }
    }

    public async Task<bool> StartAsync()
    {
        try
        {
            _logger.LogInformation("Starting SimConnect data collection");
            
#if SIMCONNECT_AVAILABLE
            if (_simConnect != null && _isConnected)
            {
                // Start requesting data at specified rate
                var intervalMs = 1000 / _updateRateHz;
                _dataUpdateTimer = new System.Threading.Timer(RequestFlightData, null, 0, intervalMs);
                
                _logger.LogInformation("SimConnect data collection timer started at {Rate}Hz", _updateRateHz);
            }
            else
            {
                _logger.LogWarning("SimConnect not connected - cannot start data collection");
                return false;
            }
#else
            // Simulation mode - generate fake data
            var intervalMs = 1000 / _updateRateHz;
            _dataUpdateTimer = new System.Threading.Timer(GenerateSimulatedData, null, 0, intervalMs);
            _isConnected = true;
            ConnectionStatusChanged?.Invoke(this, true);
            _logger.LogInformation("Started simulation mode data generation at {Rate}Hz", _updateRateHz);
#endif
            
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
            _logger.LogInformation("Stopping SimConnect data collection");
            
            // Stop the data timer
            _dataUpdateTimer?.Dispose();
            _dataUpdateTimer = null;
            
#if SIMCONNECT_AVAILABLE            
            // No additional cleanup needed for SimConnect data requests
#else
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, false);
#endif
            
            await Task.Delay(100); // Give timer time to stop
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
    {
        if (!_disposed && disposing)
        {
            _dataUpdateTimer?.Dispose();
            
#if SIMCONNECT_AVAILABLE
            try
            {
                _simConnect?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing SimConnect");
            }
#endif
            
            _disposed = true;
        }
    }

#if SIMCONNECT_AVAILABLE
    private void DefineDataStructures()
    {
        try
        {
            // Define the data structure for flight data
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "AIRSPEED INDICATED", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "VERTICAL SPEED", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "PLANE PITCH DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "PLANE BANK DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "GENERAL ENG THROTTLE LEVER POSITION:1", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "GENERAL ENG RPM:1", "rpm", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "FUEL TOTAL QUANTITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "FLAPS HANDLE PERCENT", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "GEAR HANDLE POSITION", "bool", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "AMBIENT WIND VELOCITY", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "AMBIENT WIND DIRECTION", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "G FORCE", "gforce", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "GROUND VELOCITY", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "AIRSPEED TRUE", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "INCIDENCE ALPHA", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            _simConnect?.AddToDataDefinition(DATA_DEFINITIONS.FlightDataDef, "INCIDENCE BETA", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

            // Register the data definition
            _simConnect?.RegisterDataDefineStruct<SimFlightData>(DATA_DEFINITIONS.FlightDataDef);
            
            _logger.LogDebug("SimConnect data structures defined successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to define SimConnect data structures");
            throw;
        }
    }

    private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
        _logger.LogInformation("SimConnect connection opened successfully");
        _isConnected = true;
        ConnectionStatusChanged?.Invoke(this, true);
    }

    private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
    {
        _logger.LogInformation("SimConnect connection closed by simulator");
        _isConnected = false;
        ConnectionStatusChanged?.Invoke(this, false);
    }

    private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
    {
        _logger.LogWarning("SimConnect exception: {Exception}", data.dwException);
    }    private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        try
        {
            if (data.dwRequestID == (uint)DATA_REQUESTS.FlightDataRequest)
            {
                var simData = (SimFlightData)data.dwData[0];
                
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
                        WindSpeedKnots = simData.WindVelocityX, // Simplified wind speed
                        WindDirectionDegrees = 0, // Will be calculated from wind components
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
    }

    private void RequestFlightData(object? state)
    {
        try
        {
            if (_simConnect != null && _isConnected)
            {
                _simConnect.RequestDataOnSimObject(DATA_REQUESTS.FlightDataRequest, 
                    DATA_DEFINITIONS.FlightDataDef, 
                    SimConnect.SIMCONNECT_OBJECT_ID_USER, 
                    SIMCONNECT_PERIOD.ONCE, 
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 
                    0, 0, 0);
                    
                // Process Windows messages to receive SimConnect data
                _simConnect.ReceiveMessage();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting flight data from SimConnect");
        }
    }
#else
    private void GenerateSimulatedData(object? state)
    {
        try
        {
            // Generate realistic simulated flight data for testing
            var random = new Random();
            var baseTime = DateTime.UtcNow;
            
            var flightData = new FlightData
            {
                Timestamp = baseTime,
                AirspeedKnots = 120 + random.NextDouble() * 80, // 120-200 knots
                AltitudeFeet = 5000 + random.NextDouble() * 15000, // 5000-20000 ft
                VerticalSpeedFpm = (random.NextDouble() - 0.5) * 2000, // -1000 to +1000 ft/min
                HeadingDegrees = random.NextDouble() * 360, // 0-360 degrees
                PitchAngleDegrees = (random.NextDouble() - 0.5) * 20, // -10 to +10 degrees
                BankAngleDegrees = (random.NextDouble() - 0.5) * 60, // -30 to +30 degrees
                ControlSurfaces = new ControlSurfaceData
                {
                    ElevatorPosition = (random.NextDouble() - 0.5) * 0.8, // -0.4 to +0.4
                    AileronPosition = (random.NextDouble() - 0.5) * 0.8, // -0.4 to +0.4
                    RudderPosition = (random.NextDouble() - 0.5) * 0.6 // -0.3 to +0.3
                },
                Engine = new EngineData
                {
                    ThrottlePosition = 0.6 + random.NextDouble() * 0.4, // 60-100%
                    Rpm = 2000 + random.NextDouble() * 500, // 2000-2500 RPM
                    ManifoldPressure = 25 + random.NextDouble() * 5, // 25-30 inHg
                    PropRpm = 2200 + random.NextDouble() * 300 // 2200-2500 RPM
                },
                Environment = new EnvironmentData
                {
                    WindSpeedKnots = random.NextDouble() * 30, // 0-30 knots
                    WindDirectionDegrees = random.NextDouble() * 360, // 0-360 degrees
                    Turbulence = random.NextDouble() * 0.5, // 0-50% turbulence
                    TemperatureCelsius = 10 + random.NextDouble() * 20, // 10-30°C
                    BarometricPressure = 29.5 + random.NextDouble() * 1.0 // 29.5-30.5 inHg
                },
                Aircraft = new AircraftConfiguration
                {
                    Title = "Simulated Cessna 172",
                    AircraftType = "General Aviation",
                    MaxWeightPounds = 2550,
                    CurrentWeightPounds = 2200,
                    WingSpanFeet = 36,
                    WingAreaSquareFeet = 174
                },
                OnGround = random.NextDouble() < 0.1, // On ground 10% of time
                IsStalling = random.NextDouble() < 0.05 // Stalling 5% of time
            };

            _currentFlightData = flightData;
            FlightDataReceived?.Invoke(this, flightData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating simulated flight data");
        }
    }
#endif
}

