namespace ESSPortal.Shared.Dtos.Auth;
public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
