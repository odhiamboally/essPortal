namespace ESSPortal.Application.Dtos.Auth;
public record Send2FACodeResponse(
    string UserId,
    string Provider,
    string? SelectedProvider,
    string Token,
    DateTimeOffset SentAt,
    DateTimeOffset ExpiresAt,
    bool CanResend,
    TimeSpan ResendCooldown,
    string? ReturnUrl
    );

