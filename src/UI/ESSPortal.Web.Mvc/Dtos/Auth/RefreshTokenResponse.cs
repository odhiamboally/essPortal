using System.Security.Claims;

namespace ESSPortal.Web.Mvc.Dtos.Auth;
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    string UserId,
    DateTimeOffset ExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    UserInfo UserInfo,
    List<Claim> Claims
);

