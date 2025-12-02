namespace ESSPortal.Application.Dtos.Auth;
public record ValidateResetTokenRequest(
    string Email,
    string Token);

