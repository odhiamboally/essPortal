using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IAuthService 
{
    Task<ApiResponse<bool>> RegisterEmployeeAsync(RegisterEmployeeRequest request);
    Task<ApiResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<ApiResponse<bool>> ResendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<ApiResponse<bool>> ConfirmUserEmailAsync(ConfirmUserEmailRequest confirmEmailRequest);

    Task<ApiResponse<LoginResponse>> SignInAsync(LoginRequest loginRequest);
    Task<ApiResponse<CurrentUserResponse>> GetCurrentUserAsync();


    Task<ApiResponse<ProviderResponse>> Get2FAProvidersAsync(Get2FAProviderRequest providersRequest);
    Task<ApiResponse<Send2FACodeResponse>> Send2FACodeAsync(Send2FACodeRequest sendCodeRequest);
    Task<ApiResponse<Verify2FACodeResponse>> Verify2FACodeAsync(Verify2FACodeRequest verifyCodeRequest);


    Task<ApiResponse<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<ApiResponse<bool>> ValidatePasswordResetTokenAsync(ValidateResetTokenRequest request);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<bool>> VerifyPasswordAsync(VerifyPasswordRequest verifyPasswordRequest);

    Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    Task<ApiResponse<bool>> SignOutAsync();


}
