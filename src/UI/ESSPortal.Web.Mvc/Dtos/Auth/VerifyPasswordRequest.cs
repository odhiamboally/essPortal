namespace ESSPortal.Web.Mvc.Dtos.Auth;

public record VerifyPasswordRequest(
    string UserId,
    string Email,
    string EmployeeNumber,
    string Password
);

