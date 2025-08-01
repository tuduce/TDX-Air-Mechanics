using System;
using System.Threading.Tasks;
using TDXAirMechanics.Core.Models;
using TDXAirMechanics.Core.Interfaces;

namespace TDXAirMechanics.Core.Services;

/// <summary>
/// Service for computing and sending force feedback effects
/// </summary>
public class ForceFeedbackService
{
    private readonly IDirectInputManager _directInputManager;

    public ForceFeedbackService(IDirectInputManager directInputManager)
    {
        _directInputManager = directInputManager;
    }

    /// <summary>
    /// Compute and send a spring force effect based on airspeed and barber pole
    /// </summary>
    /// <param name="indicatedAirspeed">Current indicated airspeed (knots)</param>
    /// <param name="barberPoleAirspeed">Maximum airspeed (barber pole, knots)</param>
    /// <param name="durationMs">Effect duration in ms (0 = infinite)</param>
    /// <param name="priority">Effect priority</param>
    public async Task SendSpringEffectAsync(double indicatedAirspeed, double barberPoleAirspeed, int durationMs = 0, ForcePriority priority = ForcePriority.Normal)
    {
        var parameters = ForceCalculationEngine.ComputeSpringEffectParameters(indicatedAirspeed, barberPoleAirspeed);
        var effect = new ForceFeedbackEffect
        {
            EffectType = EffectType.Spring,
            Parameters = parameters,
            DurationMs = durationMs,
            Priority = priority
        };
        await _directInputManager.ApplyEffectAsync(effect);
    }
}
