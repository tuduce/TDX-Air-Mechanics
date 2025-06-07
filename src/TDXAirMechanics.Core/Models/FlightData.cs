namespace TDXAirMechanics.Core.Models;

/// <summary>
/// Represents flight data received from Microsoft Flight Simulator via SimConnect
/// </summary>
public class FlightData
{
    /// <summary>
    /// Aircraft airspeed in knots
    /// </summary>
    public double AirspeedKnots { get; set; }

    /// <summary>
    /// Aircraft altitude in feet
    /// </summary>
    public double AltitudeFeet { get; set; }

    /// <summary>
    /// Aircraft vertical speed in feet per minute
    /// </summary>
    public double VerticalSpeedFpm { get; set; }

    /// <summary>
    /// Aircraft heading in degrees
    /// </summary>
    public double HeadingDegrees { get; set; }

    /// <summary>
    /// Bank angle in degrees (positive = right bank)
    /// </summary>
    public double BankAngleDegrees { get; set; }

    /// <summary>
    /// Pitch angle in degrees (positive = nose up)
    /// </summary>
    public double PitchAngleDegrees { get; set; }

    /// <summary>
    /// Control surface positions
    /// </summary>
    public ControlSurfaceData ControlSurfaces { get; set; } = new();

    /// <summary>
    /// Engine and propulsion data
    /// </summary>
    public EngineData Engine { get; set; } = new();

    /// <summary>
    /// Environmental conditions
    /// </summary>
    public EnvironmentData Environment { get; set; } = new();

    /// <summary>
    /// Aircraft configuration
    /// </summary>
    public AircraftConfiguration Aircraft { get; set; } = new();

    /// <summary>
    /// Timestamp of the data
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the aircraft is on the ground
    /// </summary>
    public bool OnGround { get; set; }

    /// <summary>
    /// Whether the aircraft is in a stall condition
    /// </summary>
    public bool IsStalling { get; set; }
}

/// <summary>
/// Control surface positions and forces
/// </summary>
public class ControlSurfaceData
{
    /// <summary>
    /// Elevator position (-1.0 to 1.0, positive = nose up)
    /// </summary>
    public double ElevatorPosition { get; set; }

    /// <summary>
    /// Aileron position (-1.0 to 1.0, positive = right roll)
    /// </summary>
    public double AileronPosition { get; set; }

    /// <summary>
    /// Rudder position (-1.0 to 1.0, positive = nose right)
    /// </summary>
    public double RudderPosition { get; set; }

    /// <summary>
    /// Elevator trim position
    /// </summary>
    public double ElevatorTrim { get; set; }

    /// <summary>
    /// Aileron trim position
    /// </summary>
    public double AileronTrim { get; set; }

    /// <summary>
    /// Rudder trim position
    /// </summary>
    public double RudderTrim { get; set; }
}

/// <summary>
/// Engine and propulsion related data
/// </summary>
public class EngineData
{
    /// <summary>
    /// Engine RPM
    /// </summary>
    public double Rpm { get; set; }

    /// <summary>
    /// Manifold pressure in inches of mercury
    /// </summary>
    public double ManifoldPressure { get; set; }

    /// <summary>
    /// Throttle position (0.0 to 1.0)
    /// </summary>
    public double ThrottlePosition { get; set; }

    /// <summary>
    /// Propeller RPM
    /// </summary>
    public double PropRpm { get; set; }
}

/// <summary>
/// Environmental conditions affecting flight
/// </summary>
public class EnvironmentData
{
    /// <summary>
    /// Wind speed in knots
    /// </summary>
    public double WindSpeedKnots { get; set; }

    /// <summary>
    /// Wind direction in degrees
    /// </summary>
    public double WindDirectionDegrees { get; set; }

    /// <summary>
    /// Turbulence level (0.0 to 1.0)
    /// </summary>
    public double Turbulence { get; set; }

    /// <summary>
    /// Outside air temperature in Celsius
    /// </summary>
    public double TemperatureCelsius { get; set; }

    /// <summary>
    /// Barometric pressure in inches of mercury
    /// </summary>
    public double BarometricPressure { get; set; }
}

/// <summary>
/// Aircraft configuration and characteristics
/// </summary>
public class AircraftConfiguration
{
    /// <summary>
    /// Aircraft title/name
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Aircraft type (e.g., "Cessna 172", "Boeing 737")
    /// </summary>
    public string AircraftType { get; set; } = string.Empty;

    /// <summary>
    /// Aircraft maximum weight in pounds
    /// </summary>
    public double MaxWeightPounds { get; set; }

    /// <summary>
    /// Current aircraft weight in pounds
    /// </summary>
    public double CurrentWeightPounds { get; set; }

    /// <summary>
    /// Wing span in feet
    /// </summary>
    public double WingSpanFeet { get; set; }

    /// <summary>
    /// Wing area in square feet
    /// </summary>
    public double WingAreaSquareFeet { get; set; }
}
