using ESSPortal.Application.Dtos.Profile;
using System.Security.Claims;

namespace ESSPortal.Application.Dtos.Auth;
public record RefreshTokenResponse(
    string Token,
    string RefreshToken,
    string UserId,
    DateTimeOffset TokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    UserInfo UserInfo,
    List<Claim> UserClaims
);

