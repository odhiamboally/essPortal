using EssPortal.Web.Mvc.Dtos.Common;
using LoginRequest = EssPortal.Web.Mvc.Dtos.Auth.LoginRequest;
using RegisterEmployeeRequest = EssPortal.Web.Mvc.Dtos.Auth.RegisterEmployeeRequest;
using ResetPasswordRequest = EssPortal.Web.Mvc.Dtos.Auth.ResetPasswordRequest;
using ForgotPasswordRequest = EssPortal.Web.Mvc.Dtos.Auth.ForgotPasswordRequest;
using EssPortal.Web.Mvc.Dtos.Auth;
using ESSPortal.Web.Mvc.Dtos.Auth;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IAuthService
{
    
    Task<AppResponse<bool>> RegisterEmployeeAsync(RegisterEmployeeRequest registerRequest);
    Task<AppResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<AppResponse<bool>> ResendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<AppResponse<bool>> ConfirmUserEmailAsync(ConfirmUserEmailRequest confirmEmailRequest);


    Task<AppResponse<LoginResponse>> SignInAsync(LoginRequest loginRequest);
    Task<AppResponse<SessionStatusResponse>> KeepAliveAsync();
    Task<AppResponse<UnlockResponse>> UnlockSessionAsync(UnlockRequest unlockRequest);
    Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync();

    Task<AppResponse<ProviderResponse>> Get2FAProvidersAsync(Get2FAProviderRequest providerRequest);
    Task<AppResponse<Send2FACodeResponse>> Send2FACodeAsync(Send2FACodeRequest sendCodeRequest);
    Task<AppResponse<Verify2FACodeResponse>> Verify2FACodeAsync(Verify2FACodeRequest verifyCodeRequest);

    Task<AppResponse<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<AppResponse<bool>> ValidatePasswordResetTokenAsync(ValidateResetTokenRequest request);
    Task<AppResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);


    Task<AppResponse<RefreshTokenResponse>> RefreshTokenAsync();

    Task<AppResponse<bool>> SignOutAsync();


    #region Authentication State Management
    bool IsAuthenticated();
    string? GetCurrentUserId();
    Task<bool> EnsureAuthenticatedAsync();
    Task<AppResponse<bool>> VerifyPasswordAsync(VerifyPasswordRequest verifyPasswordRequest);
    string? GetSessionId();
    
    #endregion




}
