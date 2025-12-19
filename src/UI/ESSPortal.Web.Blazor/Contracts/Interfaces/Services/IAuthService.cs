using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.Auth;
using ESSPortal.Web.Blazor.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Auth;
using ResetPasswordRequest = ESSPortal.Application.Dtos.Auth.ResetPasswordRequest;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface IAuthService
{
    
    Task<ApiResponse<bool>> RegisterEmployeeAsync(RegisterEmployeeRequest registerRequest);
    Task<ApiResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<ApiResponse<bool>> ResendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<ApiResponse<bool>> ConfirmUserEmailAsync(ConfirmUserEmailRequest confirmEmailRequest);


    Task<ApiResponse<LoginResponse>> SignInAsync(LoginRequest loginRequest);
    Task<ApiResponse<SessionStatusResponse>> KeepAliveAsync();
    Task<ApiResponse<UnlockResponse>> UnlockSessionAsync(UnlockRequest unlockRequest);
    Task<ApiResponse<CurrentUserResponse>> GetCurrentUserAsync();

    Task<ApiResponse<ProviderResponse>> Get2FAProvidersAsync(Get2FAProviderRequest providerRequest);
    Task<ApiResponse<Send2FACodeResponse>> Send2FACodeAsync(Send2FACodeRequest sendCodeRequest);
    Task<ApiResponse<Verify2FACodeResponse>> Verify2FACodeAsync(Verify2FACodeRequest verifyCodeRequest);

    Task<ApiResponse<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<ApiResponse<bool>> ValidatePasswordResetTokenAsync(ValidateResetTokenRequest request);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);


    Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync();

    Task<ApiResponse<bool>> SignOutAsync();


    #region Authentication State Management
    bool IsAuthenticated();
    string? GetCurrentUserId();
    Task<bool> EnsureAuthenticatedAsync();
    Task<ApiResponse<bool>> VerifyPasswordAsync(VerifyPasswordRequest verifyPasswordRequest);
    string? GetSessionId();
    
    #endregion




}
