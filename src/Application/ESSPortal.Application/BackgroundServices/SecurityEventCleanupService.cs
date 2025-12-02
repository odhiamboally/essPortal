using ESSPortal.Application.Configuration;
using ESSPortal.Domain.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESSPortal.Application.BackgroundServices;
public class SecurityEventCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SecurityEventCleanupService> _logger;
    private readonly BackgroundServiceSettings _settings;

    public SecurityEventCleanupService(
        IServiceProvider serviceProvider,
        ILogger<SecurityEventCleanupService> logger, 
        IOptions<BackgroundServiceSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.EnableSecurityEventCleanup)
        {
            _logger.LogInformation("Security event cleanup is disabled");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a new scope for each cleanup operation
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();

                if (unitOfWork != null)
                {
                    // Clean up old security events (keep last 90 days)
                    var cutoffDate = DateTimeOffset.UtcNow.AddDays(-90);
                    await unitOfWork.IpSecurityEventRepository.DeleteOldEventsAsync(cutoffDate);

                    _logger.LogInformation("Security event cleanup completed - removed events older than {CutoffDate}", cutoffDate);
                }
                else
                {
                    _logger.LogWarning("UnitOfWork not available for security event cleanup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during security event cleanup");
            }

            // Run daily
            await Task.Delay(TimeSpan.FromHours(_settings.SecurityEventCleanupIntervalHours), stoppingToken);
        }
    }
}