namespace ESSPortal.Application.Dtos.Auth;
public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
