using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using TDXAirMechanics.Core.Services;
using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.Tests;

[TestClass]
public class ForceCalculationEngineTests
{
    private Mock<ILogger<ForceCalculationEngine>> _mockLogger;
    private ForceCalculationEngine _forceEngine;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ForceCalculationEngine>>();
        _forceEngine = new ForceCalculationEngine(_mockLogger.Object);
    }

    [TestMethod]
    public void CalculateForces_WithValidFlightData_ReturnsForceData()
    {
        // Arrange
        var flightData = new FlightData
        {
            AirspeedKnots = 100,
            ControlSurfaces = new ControlSurfaceData
            {
                ElevatorPosition = 0.5,
                AileronPosition = 0.3
            },
            IsStalling = false,
            Environment = new EnvironmentData
            {
                Turbulence = 0.2
            }
        };

        var forceConfig = new ForceConfiguration
        {
            GlobalMultiplier = 1.0,
            AerodynamicMultiplier = 1.0,
            ApplySafetyLimits = true
        };

        // Act
        var result = _forceEngine.CalculateForces(flightData, forceConfig);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(Math.Abs(result.ForceX) <= 1.0);
        Assert.IsTrue(Math.Abs(result.ForceY) <= 1.0);
        Assert.AreEqual(ForceType.Aerodynamic, result.Type);
    }

    [TestMethod]
    public void CalculateForces_WithStalling_IncludesStallForces()
    {
        // Arrange
        var flightData = new FlightData
        {
            AirspeedKnots = 50,
            IsStalling = true,
            ControlSurfaces = new ControlSurfaceData(),
            Environment = new EnvironmentData()
        };

        var forceConfig = new ForceConfiguration
        {
            StallMultiplier = 2.0
        };

        // Act
        var result = _forceEngine.CalculateForces(flightData, forceConfig);

        // Assert
        Assert.IsNotNull(result);
        // Stall forces should create some vibration
        Assert.IsTrue(result.ForceX != 0 || result.ForceY != 0);
    }

    [TestMethod]
    public void SetAircraftProfile_ValidProfile_SetsCurrentProfile()
    {
        // Arrange
        var profile = new AircraftForceProfile
        {
            AircraftId = "Test Aircraft",
            DisplayName = "Test Aircraft Profile"
        };

        // Act
        _forceEngine.SetAircraftProfile(profile);

        // Assert
        Assert.AreEqual(profile, _forceEngine.CurrentProfile);
    }
}

[TestClass]
public class ConfigurationManagerTests
{
    private Mock<ILogger<ConfigurationManager>> _mockLogger;
    private ConfigurationManager _configManager;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ConfigurationManager>>();
        _configManager = new ConfigurationManager(_mockLogger.Object);
    }

    [TestMethod]
    public void CreateDefaultConfiguration_ReturnsValidConfiguration()
    {
        // Act
        var config = _configManager.CreateDefaultConfiguration();

        // Assert
        Assert.IsNotNull(config);
        Assert.IsNotNull(config.General);
        Assert.IsNotNull(config.SimConnect);
        Assert.IsNotNull(config.DirectInput);
        Assert.IsNotNull(config.ForceSettings);
        Assert.IsNotNull(config.UI);
        
        // Verify some default values
        Assert.AreEqual(30, config.SimConnect.UpdateRateHz);
        Assert.AreEqual(true, config.General.AutoConnect);
        Assert.AreEqual(0.85, config.ForceSettings.MaxForceLimit);
    }
}
