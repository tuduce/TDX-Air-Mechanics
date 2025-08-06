using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.Core.Interfaces
{
    /// <summary>
    /// Interface for calculating force feedback from flight data
    /// </summary>
    public interface IForceCalculationEngine
    {
        /// <summary>
        /// Calculate force feedback data from flight data
        /// </summary>
        /// <param name="flightData">Current flight data</param>
        /// <param name="forceConfig">Force configuration to use</param>
        /// <returns>Calculated force feedback data</returns>
        ForceFeedbackData CalculateForces(FlightData flightData, ForceConfiguration forceConfig);

        /// <summary>
        /// Set the active aircraft profile
        /// </summary>
        /// <param name="profile">Aircraft force profile</param>
        void SetAircraftProfile(AircraftForceProfile profile);

        /// <summary>
        /// Get the currently active aircraft profile
        /// </summary>
        AircraftForceProfile? CurrentProfile { get; }

        /// <summary>
        /// Update force calculation parameters
        /// </summary>
        /// <param name="parameters">New parameters</param>
        void UpdateParameters(Dictionary<string, object> parameters);
    }
}
