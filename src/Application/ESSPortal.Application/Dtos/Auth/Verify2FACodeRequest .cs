namespace ESSPortal.Application.Dtos.Auth;
public record Verify2FACodeRequest(
    string UserId,
    string Provider,
    string Code,
    bool RememberMe,
    bool RememberDevice,
    bool RememberBrowser,
    string? DeviceFingerprint,
    string ProviderDisplayName,
    string? ReturnUrl

);
