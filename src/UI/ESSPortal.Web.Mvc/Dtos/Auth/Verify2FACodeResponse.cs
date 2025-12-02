using ESSPortal.Web.Mvc.Dtos.Auth;

namespace EssPortal.Web.Mvc.Dtos.Auth;

public record Verify2FACodeResponse
{
    public string Token { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public bool IsAuthenticated { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public string? RefreshToken { get; init; }
    public UserInfo? UserInfo { get; init; }
    public List<UserClaimsResponse>? UserClaims { get; init; }
}
