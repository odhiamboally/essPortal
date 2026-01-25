namespace EssPortal.Shared.Dtos.Auth;

public record ConfirmUserEmailRequest(string Email, string Token);
