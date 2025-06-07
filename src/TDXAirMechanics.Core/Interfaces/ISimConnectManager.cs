using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.Core.Interfaces;

/// <summary>
/// Interface for SimConnect data acquisition
/// </summary>
public interface ISimConnectManager : IDisposable
{
    /// <summary>
    /// Event fired when new flight data is received
    /// </summary>
    event EventHandler<FlightData>? FlightDataReceived;

    /// <summary>
    /// Event fired when connection status changes
    /// </summary>
    event EventHandler<bool>? ConnectionStatusChanged;

    /// <summary>
    /// Initialize the SimConnect connection
    /// </summary>
    /// <returns>True if initialization was successful</returns>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Start data collection
    /// </summary>
    /// <returns>True if started successfully</returns>
    Task<bool> StartAsync();

    /// <summary>
    /// Stop data collection
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Check if currently connected to MSFS
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Get the current flight data
    /// </summary>
    FlightData? CurrentFlightData { get; }

    /// <summary>
    /// Data update rate in Hz
    /// </summary>
    int UpdateRateHz { get; set; }
}
