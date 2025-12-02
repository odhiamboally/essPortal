namespace ESSPortal.Application.Configuration;


public class SessionManagementSettings
{
    public int MaxConcurrentSessions { get; set; } = 1;
    public int SessionTimeoutMinutes { get; set; } = 15;
    public bool SlidingExpiration { get; set; } = true;
    public int CleanupIntervalMinutes { get; set; } = 30;
    public bool EnableConcurrentSessionNotification { get; set; } = true;
    public bool UseLockScreen { get; set; } = true;
}
