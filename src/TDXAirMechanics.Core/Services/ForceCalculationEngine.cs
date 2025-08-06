using Microsoft.Extensions.Logging;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.Core.Services;

/// <summary>
/// Force calculation engine implementation
/// </summary>
public class ForceCalculationEngine : IForceCalculationEngine
{
    private readonly ILogger<ForceCalculationEngine> _logger;
    private AircraftForceProfile? _currentProfile;

    public ForceCalculationEngine(ILogger<ForceCalculationEngine> logger)
    {
        _logger = logger;
    }

    public AircraftForceProfile? CurrentProfile => _currentProfile;

    public ForceFeedbackData CalculateForces(FlightData flightData, ForceConfiguration forceConfig)
    {
        var forceData = new ForceFeedbackData
        {
            Timestamp = DateTime.UtcNow,
            Type = ForceType.Aerodynamic,
            Priority = ForcePriority.Normal
        };

        return forceData;
    }

    public void SetAircraftProfile(AircraftForceProfile profile)
    {
        _currentProfile = profile;
        _logger.LogInformation("Set aircraft profile: {ProfileName}", profile.DisplayName);
    }

    public void UpdateParameters(Dictionary<string, object> parameters)
    {
        _logger.LogDebug("Updated force calculation parameters: {Count} parameters", parameters.Count);
        // TODO: Implement parameter updates
    }

    /// <summary>
    /// Compute spring effect parameters based on airspeed and barber pole
    /// </summary>
    /// <param name="indicatedAirspeed">Current indicated airspeed (knots)</param>
    /// <param name="barberPoleAirspeed">Maximum airspeed (barber pole, knots)</param>
    /// <returns>Dictionary of spring effect parameters</returns>
    public static Dictionary<string, object> ComputeSpringEffectParameters(double indicatedAirspeed, double barberPoleAirspeed)
    {
        // Normalize airspeed (0.0 = stopped, 1.0 = barber pole)
        double normalized = barberPoleAirspeed > 0 ? Math.Clamp(indicatedAirspeed / barberPoleAirspeed, 0.0, 1.0) : 0.0;
        // Example: spring strength increases with airspeed, up to a max
        double minStrength = 0.1;
        double maxStrength = 1.0;
        double strength = minStrength + (maxStrength - minStrength) * normalized;
        // Center point (0,0) for joystick
        double centerX = 0.0;
        double centerY = 0.0;
        return new Dictionary<string, object>
        {
            { "Strength", strength },
            { "CenterX", centerX },
            { "CenterY", centerY }
        };
    }
}
