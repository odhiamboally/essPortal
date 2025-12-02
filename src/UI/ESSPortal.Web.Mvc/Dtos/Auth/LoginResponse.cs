using ESSPortal.Web.Mvc.Dtos.Auth;

namespace EssPortal.Web.Mvc.Dtos.Auth;


public record LoginResponse(
    string UserId,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    bool Requires2FA,
    bool RequiresEmailConfirmation,
    bool IsAuthenticated,
    string Token,
    string RefreshToken,
    DateTimeOffset TokenExpiresAt,
    UserInfo? UserInfo,
    List<ClaimResponse>? UserClaims

    );
