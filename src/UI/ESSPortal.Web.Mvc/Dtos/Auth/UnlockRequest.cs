namespace ESSPortal.Web.Mvc.Dtos.Auth;

public record UnlockRequest(string Password, string? Email = null, string? EmployeeNumber = null);
