namespace EssPortal.Web.Mvc.Dtos.Auth;


public record Send2FACodeResponse
{
    public string UserId { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string MaskedDestination { get; init; } = string.Empty; // e.g., "***@example.com" or "***-***-1234"
    public DateTimeOffset SentAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public int CodeLength { get; init; } = 6;
    public bool CanResend { get; init; }
    public TimeSpan ResendCooldown { get; init; }

    // For development/testing - remove in production
    public string? Token { get; init; } // Only for dev environment
    public string? ReturnUrl { get; init; } // Only for dev environment
    
}

