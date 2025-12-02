namespace EssPortal.Web.Mvc.Dtos.Auth;



public record Verify2FACodeRequest
{
    public string UserId { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string ProviderDisplayName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public bool RememberMe { get; init; }
    public bool RememberDevice { get; init; } = true;
    public bool RememberBrowser { get; init; } = true;
    public string? DeviceFingerprint { get; init; }
    public string? ReturnUrl { get; init; }
}