namespace EssPortal.Shared.Dtos.Auth;


public record Send2FACodeResponse
{
    public string UserId { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? SelectedProvider { get; init; }
    public string MaskedDestination { get; init; } = string.Empty;
    public string? Token { get; init; }
    public DateTimeOffset SentAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public int CodeLength { get; init; } = 6;
    public bool CanResend { get; init; }
    public TimeSpan ResendCooldown { get; init; }
    public string? ReturnUrl { get; init; }
}
