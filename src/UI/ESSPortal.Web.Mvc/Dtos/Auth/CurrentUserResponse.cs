namespace EssPortal.Web.Mvc.Dtos.Auth;

public record CurrentUserResponse(
    string EmployeeNumber,
    string UserId,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    string Gender,
    bool? IsAuthenticated,
    DateTimeOffset? LastLoginAt,
    List<string> Roles);
