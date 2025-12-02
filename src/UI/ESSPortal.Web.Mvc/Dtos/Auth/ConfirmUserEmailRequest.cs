namespace EssPortal.Web.Mvc.Dtos.Auth;

public record ConfirmUserEmailRequest(string Email, string Token);
