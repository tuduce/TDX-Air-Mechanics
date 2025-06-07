namespace TDXAirMechanics.Core.Models;

/// <summary>
/// Represents force feedback data to be sent to the joystick
/// </summary>
public class ForceFeedbackData
{
    /// <summary>
    /// X-axis force (-1.0 to 1.0, positive = right)
    /// </summary>
    public double ForceX { get; set; }

    /// <summary>
    /// Y-axis force (-1.0 to 1.0, positive = forward/up)
    /// </summary>
    public double ForceY { get; set; }

    /// <summary>
    /// Timestamp when the force was calculated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Duration the force should be applied in milliseconds
    /// </summary>
    public int DurationMs { get; set; } = 50;

    /// <summary>
    /// Priority of this force effect (higher = more important)
    /// </summary>
    public ForcePriority Priority { get; set; } = ForcePriority.Normal;

    /// <summary>
    /// Type of force effect being applied
    /// </summary>
    public ForceType Type { get; set; }

    /// <summary>
    /// Clamp forces to valid range
    /// </summary>
    public void ClampForces()
    {
        ForceX = Math.Max(-1.0, Math.Min(1.0, ForceX));
        ForceY = Math.Max(-1.0, Math.Min(1.0, ForceY));
    }
}

/// <summary>
/// Types of force effects
/// </summary>
public enum ForceType
{
    /// <summary>
    /// Aerodynamic forces from control surfaces
    /// </summary>
    Aerodynamic,
    
    /// <summary>
    /// Stall buffet and warning forces
    /// </summary>
    Stall,
    
    /// <summary>
    /// Turbulence effects
    /// </summary>
    Turbulence,
    
    /// <summary>
    /// Ground effect forces
    /// </summary>
    GroundEffect,
    
    /// <summary>
    /// Engine-induced vibrations
    /// </summary>
    Engine,
    
    /// <summary>
    /// Landing gear effects
    /// </summary>
    LandingGear,
    
    /// <summary>
    /// Custom user-defined effects
    /// </summary>
    Custom
}

/// <summary>
/// Force priority levels
/// </summary>
public enum ForcePriority
{
    /// <summary>
    /// Low priority effects
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// Normal priority effects
    /// </summary>
    Normal = 2,
    
    /// <summary>
    /// High priority effects (safety-related)
    /// </summary>
    High = 3,
    
    /// <summary>
    /// Critical priority effects (stall warnings, etc.)
    /// </summary>
    Critical = 4
}

/// <summary>
/// Configuration for force feedback effects
/// </summary>
public class ForceConfiguration
{
    /// <summary>
    /// Overall force multiplier (0.0 to 2.0)
    /// </summary>
    public double GlobalMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Aerodynamic force multiplier
    /// </summary>
    public double AerodynamicMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Stall effect multiplier
    /// </summary>
    public double StallMultiplier { get; set; } = 1.5;

    /// <summary>
    /// Turbulence effect multiplier
    /// </summary>
    public double TurbulenceMultiplier { get; set; } = 0.8;

    /// <summary>
    /// Maximum force limit (0.0 to 1.0)
    /// </summary>
    public double MaxForceLimit { get; set; } = 0.85;

    /// <summary>
    /// Minimum force threshold below which forces are ignored
    /// </summary>
    public double MinForceThreshold { get; set; } = 0.05;

    /// <summary>
    /// Whether to apply safety limits
    /// </summary>
    public bool ApplySafetyLimits { get; set; } = true;

    /// <summary>
    /// Force smoothing factor (0.0 = no smoothing, 1.0 = maximum smoothing)
    /// </summary>
    public double SmoothingFactor { get; set; } = 0.3;
}

/// <summary>
/// Aircraft-specific force profile
/// </summary>
public class AircraftForceProfile
{
    /// <summary>
    /// Aircraft identifier (title or type)
    /// </summary>
    public string AircraftId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this profile
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Force configuration for this aircraft
    /// </summary>
    public ForceConfiguration ForceConfig { get; set; } = new();

    /// <summary>
    /// Control surface sensitivity factors
    /// </summary>
    public ControlSurfaceSensitivity Sensitivity { get; set; } = new();

    /// <summary>
    /// Whether this profile is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Custom force curves for this aircraft
    /// </summary>
    public Dictionary<string, ForceCurve> CustomCurves { get; set; } = new();
}

/// <summary>
/// Control surface sensitivity settings
/// </summary>
public class ControlSurfaceSensitivity
{
    /// <summary>
    /// Elevator sensitivity multiplier
    /// </summary>
    public double ElevatorSensitivity { get; set; } = 1.0;

    /// <summary>
    /// Aileron sensitivity multiplier
    /// </summary>
    public double AileronSensitivity { get; set; } = 1.0;

    /// <summary>
    /// Rudder sensitivity multiplier
    /// </summary>
    public double RudderSensitivity { get; set; } = 1.0;
}

/// <summary>
/// Force curve definition for custom force responses
/// </summary>
public class ForceCurve
{
    /// <summary>
    /// Name of the force curve
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Input-output points defining the curve
    /// </summary>
    public List<CurvePoint> Points { get; set; } = new();

    /// <summary>
    /// Interpolate force value based on input
    /// </summary>
    /// <param name="input">Input value</param>
    /// <returns>Interpolated output value</returns>
    public double Interpolate(double input)
    {
        if (Points.Count == 0) return 0.0;
        if (Points.Count == 1) return Points[0].Output;

        // Find surrounding points
        var sortedPoints = Points.OrderBy(p => p.Input).ToList();
        
        if (input <= sortedPoints[0].Input) return sortedPoints[0].Output;
        if (input >= sortedPoints[^1].Input) return sortedPoints[^1].Output;

        for (int i = 0; i < sortedPoints.Count - 1; i++)
        {
            if (input >= sortedPoints[i].Input && input <= sortedPoints[i + 1].Input)
            {
                // Linear interpolation
                var t = (input - sortedPoints[i].Input) / (sortedPoints[i + 1].Input - sortedPoints[i].Input);
                return sortedPoints[i].Output + t * (sortedPoints[i + 1].Output - sortedPoints[i].Output);
            }
        }

        return 0.0;
    }
}

/// <summary>
/// Point on a force curve
/// </summary>
public class CurvePoint
{
    /// <summary>
    /// Input value
    /// </summary>
    public double Input { get; set; }

    /// <summary>
    /// Output value
    /// </summary>
    public double Output { get; set; }

    public CurvePoint() { }

    public CurvePoint(double input, double output)
    {
        Input = input;
        Output = output;
    }
}
