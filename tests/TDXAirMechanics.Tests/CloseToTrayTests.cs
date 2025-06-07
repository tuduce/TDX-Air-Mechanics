using Microsoft.Extensions.Logging.Abstractions;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.Core.Models;
using TDXAirMechanics.Core.Services;
using Xunit;

namespace TDXAirMechanics.Tests;

public class CloseToTrayTests
{
    [Fact]
    public async Task ConfigurationManager_Should_Load_Default_CloseToTrayOnExit_Setting()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        File.Delete(tempPath); // Delete the file so we test default creation
        
        var configManager = new ConfigurationManager(NullLogger<ConfigurationManager>.Instance, tempPath);

        // Act
        var config = await configManager.LoadConfigurationAsync();

        // Assert
        Assert.True(config.General.CloseToTrayOnExit, "Default CloseToTrayOnExit setting should be true");
        
        // Cleanup
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task ConfigurationManager_Should_Save_And_Load_CloseToTrayOnExit_Setting()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        var configManager = new ConfigurationManager(NullLogger<ConfigurationManager>.Instance, tempPath);

        // Act - Load default config and modify setting
        var config = await configManager.LoadConfigurationAsync();
        config.General.CloseToTrayOnExit = false;
        await configManager.SaveConfigurationAsync(config);

        // Create a new instance to test loading from file
        var configManager2 = new ConfigurationManager(NullLogger<ConfigurationManager>.Instance, tempPath);
        var loadedConfig = await configManager2.LoadConfigurationAsync();

        // Assert
        Assert.False(loadedConfig.General.CloseToTrayOnExit, "Saved CloseToTrayOnExit setting should be false");
        
        // Cleanup
        File.Delete(tempPath);
    }

    [Fact]
    public void GeneralSettings_Should_Have_Default_CloseToTrayOnExit_True()
    {
        // Arrange & Act
        var generalSettings = new GeneralSettings();

        // Assert
        Assert.True(generalSettings.CloseToTrayOnExit, "Default CloseToTrayOnExit should be true");
    }
}
