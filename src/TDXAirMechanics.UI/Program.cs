using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows.Forms;
using TDXAirMechanics.Core.Interfaces;
using TDXAirMechanics.UI.Forms;
using TDXAirMechanics.UI.Services;

namespace TDXAirMechanics.UI;

/// <summary>
/// Main entry point for the TDX Air Mechanics application
/// </summary>
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {        // Setup Serilog logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/tdx-air-mechanics-.log", 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting TDX Air Mechanics application");

            // Enable visual styles for Windows Forms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Create and configure the host
            var host = CreateHostBuilder().Build();

            // Start the host services
            await host.StartAsync();

            // Get the main form from DI container
            var mainForm = host.Services.GetRequiredService<MainForm>();

            // Run the Windows Forms application
            Application.Run(mainForm);

            // Stop the host when the application exits
            await host.StopAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            MessageBox.Show($"Fatal error: {ex.Message}", "TDX Air Mechanics", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Create and configure the host builder
    /// </summary>
    /// <returns>Configured host builder</returns>
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .UseWindowsService()
            .ConfigureServices((context, services) =>
            {
                // Register core services
                ConfigureServices(services);
            });
    }    
    
    /// <summary>
    /// Configure dependency injection services
    /// </summary>
    /// <param name="services">Service collection</param>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register Windows Forms
        services.AddSingleton<MainForm>();

        // Register application services
        services.AddSingleton<IApplicationService, ApplicationService>();
        services.AddSingleton<IStatusService, StatusService>();        // Register core interfaces
        services.AddSingleton<IConfigurationManager, TDXAirMechanics.Core.Services.ConfigurationManager>();
        services.AddSingleton<IForceCalculationEngine, TDXAirMechanics.Core.Services.ForceCalculationEngine>();
        
        // Register SimConnect manager if available
        RegisterSimConnectManager(services);
        
        // Register DirectInput manager if available
        RegisterDirectInputManager(services);
        
        // Background services
        services.AddHostedService<ApplicationBackgroundService>();
        
        // Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });
    }

    /// <summary>
    /// Register SimConnect manager if the assembly is available
    /// </summary>
    private static void RegisterSimConnectManager(IServiceCollection services)
    {
        try
        {
            // Try to load the SimConnect assembly and register the manager
            var simConnectType = Type.GetType("TDXAirMechanics.SimConnect.Services.SimConnectManager, TDXAirMechanics.SimConnect");
            if (simConnectType != null)
            {
                services.AddSingleton(typeof(ISimConnectManager), simConnectType);
                Log.Information("SimConnect manager registered successfully");
            }
            else
            {
                Log.Warning("SimConnect manager type not found - running without SimConnect support");
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to register SimConnect manager - running without SimConnect support");
        }
    }

    /// <summary>
    /// Register DirectInput manager if the assembly is available
    /// </summary>
    private static void RegisterDirectInputManager(IServiceCollection services)
    {
        try
        {
            // Try to load the DirectInput assembly and register the manager
            var directInputType = Type.GetType("TDXAirMechanics.DirectInput.Services.DirectInputManager, TDXAirMechanics.DirectInput");
            if (directInputType != null)
            {
                services.AddSingleton(typeof(IDirectInputManager), directInputType);
                Log.Information("DirectInput manager registered successfully");
            }
            else
            {
                Log.Warning("DirectInput manager type not found - running without force feedback support");
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to register DirectInput manager - running without force feedback support");
        }
    }
}
