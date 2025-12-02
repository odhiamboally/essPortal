namespace ESSPortal.Web.Mvc.Dtos.Auth;
public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
