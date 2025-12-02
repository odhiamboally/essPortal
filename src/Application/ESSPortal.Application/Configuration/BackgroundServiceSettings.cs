namespace ESSPortal.Application.Configuration;

public class BackgroundServiceSettings
{
    public bool EnableSessionCleanup { get; internal set; }
    public bool EnableSecurityEventCleanup { get; internal set; }
    public long SessionCleanupIntervalMinutes { get; internal set; }
    public int SecurityEventCleanupIntervalHours { get; internal set; }
    public int RetainSecurityEventsForDays { get; set; } = 30;
}
