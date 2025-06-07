using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TDXAirMechanics.UI.Services;

/// <summary>
/// Background service for the TDX Air Mechanics application
/// </summary>
public class ApplicationBackgroundService : BackgroundService
{
    private readonly ILogger<ApplicationBackgroundService> _logger;
    private readonly IApplicationService _applicationService;

    public ApplicationBackgroundService(
        ILogger<ApplicationBackgroundService> logger,
        IApplicationService applicationService)
    {
        _logger = logger;
        _applicationService = applicationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Application background service starting");

        try
        {
            // Start the main application services
            await _applicationService.StartAsync();

            // Keep the service running until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                // Perform periodic tasks here
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
            _logger.LogInformation("Application background service cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application background service encountered an error");
        }
        finally
        {
            // Stop the application services
            await _applicationService.StopAsync();
            _logger.LogInformation("Application background service stopped");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application background service stopping");
        await base.StopAsync(cancellationToken);
    }
}
