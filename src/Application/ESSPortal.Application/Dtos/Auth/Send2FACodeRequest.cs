namespace ESSPortal.Application.Dtos.Auth;
public record Send2FACodeRequest(
    string UserId, 
    string SelectedProvider,
    string ReturnUrl,
    bool RememberMe);
