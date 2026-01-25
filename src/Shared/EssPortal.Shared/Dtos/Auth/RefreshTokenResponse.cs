using System.Security.Claims;

namespace ESSPortal.Shared.Dtos.Auth;
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    string UserId,
    DateTimeOffset ExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    UserInfo UserInfo,
    List<Claim> UserClaims
);

