using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESSPortal.Application.BackgroundServices;
public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly BackgroundServiceSettings _settings;

    public SessionCleanupService(
        IServiceProvider serviceProvider,
        ILogger<SessionCleanupService> logger,
        IConfiguration configuration,
        IOptions<BackgroundServiceSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.EnableSessionCleanup)
        {
            _logger.LogInformation("Session cleanup is disabled");
            return;
        }
        
        _logger.LogInformation("Session cleanup service started");


        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a new scope for each cleanup operation
                using var scope = _serviceProvider.CreateScope();
                var sessionService = scope.ServiceProvider.GetService<ISessionManagementService>();

                if (sessionService != null)
                {
                    var result = await sessionService.CleanupExpiredSessionsAsync();
                    if (result.Successful)
                    {
                        _logger.LogInformation("Session cleanup completed: {Message}", result.Message);
                    }
                    else
                    {
                        _logger.LogWarning("Session cleanup failed: {Message}", result.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("SessionManagementService not available for cleanup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }

            await Task.Delay(TimeSpan.FromMinutes(_settings.SessionCleanupIntervalMinutes), stoppingToken);
        }
    }
}
