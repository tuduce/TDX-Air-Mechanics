using Microsoft.Extensions.Logging;
using System.Text.Json;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.Core.Models;

namespace TDXAirMechanics.Core.Services;

/// <summary>
/// Implementation of configuration management
/// </summary>
public class ConfigurationManager : IConfigurationManager
{
    private readonly ILogger<ConfigurationManager> _logger;
    private readonly string _configDirectory;
    private readonly string _configFile;
    private readonly string _profilesFile;

    public ConfigurationManager(ILogger<ConfigurationManager> logger)
    {
        _logger = logger;
        
        // Setup configuration directory
        _configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TDXAirMechanics");
        
        _configFile = Path.Combine(_configDirectory, "config.json");
        _profilesFile = Path.Combine(_configDirectory, "aircraft-profiles.json");
        
        // Ensure directory exists
        Directory.CreateDirectory(_configDirectory);
    }

    public async Task<AppConfiguration> LoadConfigurationAsync()
    {
        try
        {
            if (!File.Exists(_configFile))
            {
                _logger.LogInformation("Configuration file not found, creating default configuration");
                var defaultConfig = CreateDefaultConfiguration();
                await SaveConfigurationAsync(defaultConfig);
                return defaultConfig;
            }

            var json = await File.ReadAllTextAsync(_configFile);
            var config = JsonSerializer.Deserialize<AppConfiguration>(json);
            
            if (config == null)
            {
                _logger.LogWarning("Failed to deserialize configuration, using default");
                return CreateDefaultConfiguration();
            }

            _logger.LogInformation("Configuration loaded successfully");
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration");
            return CreateDefaultConfiguration();
        }
    }

    public async Task SaveConfigurationAsync(AppConfiguration config)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(_configFile, json);
            
            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration");
            throw;
        }
    }

    public async Task<List<AircraftForceProfile>> LoadAircraftProfilesAsync()
    {
        try
        {
            if (!File.Exists(_profilesFile))
            {
                _logger.LogInformation("Aircraft profiles file not found, creating default profiles");
                var defaultProfiles = CreateDefaultAircraftProfiles();
                await SaveAircraftProfilesAsync(defaultProfiles);
                return defaultProfiles;
            }

            var json = await File.ReadAllTextAsync(_profilesFile);
            var profiles = JsonSerializer.Deserialize<List<AircraftForceProfile>>(json);
            
            if (profiles == null)
            {
                _logger.LogWarning("Failed to deserialize aircraft profiles, using defaults");
                return CreateDefaultAircraftProfiles();
            }

            _logger.LogInformation("Loaded {Count} aircraft profiles", profiles.Count);
            return profiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load aircraft profiles");
            return CreateDefaultAircraftProfiles();
        }
    }

    public async Task SaveAircraftProfilesAsync(List<AircraftForceProfile> profiles)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(profiles, options);
            await File.WriteAllTextAsync(_profilesFile, json);
            
            _logger.LogInformation("Saved {Count} aircraft profiles", profiles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save aircraft profiles");
            throw;
        }
    }

    public async Task<AircraftForceProfile?> GetAircraftProfileAsync(string aircraftTitle)
    {
        var profiles = await LoadAircraftProfilesAsync();
        return profiles.FirstOrDefault(p => 
            p.AircraftId.Equals(aircraftTitle, StringComparison.OrdinalIgnoreCase));
    }

    public AppConfiguration CreateDefaultConfiguration()
    {
        return new AppConfiguration
        {            General = new GeneralSettings
            {
                StartWithWindows = false,
                MinimizeToTray = true,
                CloseToTrayOnExit = true,
                AutoConnect = true,
                Theme = "Dark",
                LogLevel = "Information"
            },
            SimConnect = new SimConnectSettings
            {
                UpdateRateHz = 30,
                ConnectionTimeoutSeconds = 10,
                AutoReconnect = true,
                ReconnectDelaySeconds = 5
            },
            DirectInput = new DirectInputSettings
            {
                AutoSelectDevice = true,
                EffectUpdateRateHz = 60,
                EnableDeviceMonitoring = true
            },
            ForceSettings = new ForceConfiguration
            {
                GlobalMultiplier = 1.0,
                AerodynamicMultiplier = 1.0,
                StallMultiplier = 1.5,
                TurbulenceMultiplier = 0.8,
                MaxForceLimit = 0.85,
                MinForceThreshold = 0.05,
                ApplySafetyLimits = true,
                SmoothingFactor = 0.3
            },
            UI = new UISettings
            {
                MainWindow = new WindowSettings
                {
                    Width = 900,
                    Height = 700,
                    X = 100,
                    Y = 100
                },
                ShowAdvancedOptions = false,
                RefreshRateHz = 10
            }
        };
    }

    private List<AircraftForceProfile> CreateDefaultAircraftProfiles()
    {
        return new List<AircraftForceProfile>
        {
            new AircraftForceProfile
            {
                AircraftId = "Cessna 172",
                DisplayName = "Cessna 172 Skyhawk",
                IsActive = true,
                ForceConfig = new ForceConfiguration
                {
                    GlobalMultiplier = 1.0,
                    AerodynamicMultiplier = 0.8,
                    StallMultiplier = 1.2
                },
                Sensitivity = new ControlSurfaceSensitivity
                {
                    ElevatorSensitivity = 1.0,
                    AileronSensitivity = 0.9,
                    RudderSensitivity = 0.7
                }
            },
            new AircraftForceProfile
            {
                AircraftId = "Default",
                DisplayName = "Default Profile",
                IsActive = true,
                ForceConfig = new ForceConfiguration(),
                Sensitivity = new ControlSurfaceSensitivity()
            }
        };
    }
}
