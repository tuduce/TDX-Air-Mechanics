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
    private ForceFeedbackData? _lastForceData;

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

        try
        {
            // Calculate aerodynamic forces
            var aeroForces = CalculateAerodynamicForces(flightData, forceConfig);
            
            // Calculate stall effects
            var stallForces = CalculateStallForces(flightData, forceConfig);
            
            // Calculate turbulence effects
            var turbulenceForces = CalculateTurbulenceForces(flightData, forceConfig);
            
            // Combine all forces
            forceData.ForceX = (aeroForces.X + stallForces.X + turbulenceForces.X) * forceConfig.GlobalMultiplier;
            forceData.ForceY = (aeroForces.Y + stallForces.Y + turbulenceForces.Y) * forceConfig.GlobalMultiplier;
            
            // Apply smoothing
            if (_lastForceData != null && forceConfig.SmoothingFactor > 0)
            {
                forceData = ApplySmoothing(forceData, _lastForceData, forceConfig.SmoothingFactor);
            }
            
            // Apply safety limits
            if (forceConfig.ApplySafetyLimits)
            {
                ApplySafetyLimits(forceData, forceConfig);
            }
            
            // Store for next smoothing calculation
            _lastForceData = forceData;
            
            _logger.LogDebug("Calculated forces: X={ForceX:F3}, Y={ForceY:F3}", 
                forceData.ForceX, forceData.ForceY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating forces");
            // Return zero forces on error
            forceData.ForceX = 0;
            forceData.ForceY = 0;
        }

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

    private (double X, double Y) CalculateAerodynamicForces(FlightData flightData, ForceConfiguration config)
    {
        var sensitivity = _currentProfile?.Sensitivity ?? new ControlSurfaceSensitivity();
        
        // Calculate control surface loading based on airspeed and control positions
        var airspeedFactor = Math.Min(flightData.AirspeedKnots / 100.0, 2.0); // Normalize and cap
        
        // Elevator forces (pitch control)
        var elevatorForce = flightData.ControlSurfaces.ElevatorPosition * 
                           airspeedFactor * 
                           sensitivity.ElevatorSensitivity * 
                           config.AerodynamicMultiplier;
        
        // Aileron forces (roll control)
        var aileronForce = flightData.ControlSurfaces.AileronPosition * 
                          airspeedFactor * 
                          sensitivity.AileronSensitivity * 
                          config.AerodynamicMultiplier;
        
        return (aileronForce * 0.5, elevatorForce * 0.7);
    }

    private (double X, double Y) CalculateStallForces(FlightData flightData, ForceConfiguration config)
    {
        if (!flightData.IsStalling) return (0, 0);
        
        // Create stall buffet effect
        var stallIntensity = config.StallMultiplier * 0.3;
        var randomX = (Random.Shared.NextDouble() - 0.5) * stallIntensity;
        var randomY = (Random.Shared.NextDouble() - 0.5) * stallIntensity;
        
        return (randomX, randomY);
    }

    private (double X, double Y) CalculateTurbulenceForces(FlightData flightData, ForceConfiguration config)
    {
        if (flightData.Environment.Turbulence <= 0) return (0, 0);
        
        // Create turbulence effects
        var turbulenceIntensity = flightData.Environment.Turbulence * config.TurbulenceMultiplier * 0.2;
        var randomX = (Random.Shared.NextDouble() - 0.5) * turbulenceIntensity;
        var randomY = (Random.Shared.NextDouble() - 0.5) * turbulenceIntensity;
        
        return (randomX, randomY);
    }

    private ForceFeedbackData ApplySmoothing(ForceFeedbackData current, ForceFeedbackData previous, double smoothingFactor)
    {
        // Simple exponential smoothing
        var alpha = 1.0 - smoothingFactor;
        
        current.ForceX = alpha * current.ForceX + smoothingFactor * previous.ForceX;
        current.ForceY = alpha * current.ForceY + smoothingFactor * previous.ForceY;
        
        return current;
    }

    private void ApplySafetyLimits(ForceFeedbackData forceData, ForceConfiguration config)
    {
        // Apply maximum force limit
        forceData.ForceX = Math.Max(-config.MaxForceLimit, Math.Min(config.MaxForceLimit, forceData.ForceX));
        forceData.ForceY = Math.Max(-config.MaxForceLimit, Math.Min(config.MaxForceLimit, forceData.ForceY));
        
        // Apply minimum threshold
        if (Math.Abs(forceData.ForceX) < config.MinForceThreshold)
            forceData.ForceX = 0;
        if (Math.Abs(forceData.ForceY) < config.MinForceThreshold)
            forceData.ForceY = 0;
    }
}
