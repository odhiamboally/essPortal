using ESSPortal.Application.Dtos.Profile;

namespace ESSPortal.Application.Dtos.Auth;
public record Verify2FACodeResponse(
    string Token, 
    string RefreshToken, 
    string UserId, 
    bool IsAuthenticated, 
    DateTimeOffset ExpiresAt, 
    UserInfo UserInfo, 
    List<UserClaimsResponse>? UserClaims
    );
