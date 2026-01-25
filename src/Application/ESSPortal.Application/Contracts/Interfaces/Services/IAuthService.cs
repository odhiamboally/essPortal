using EssPortal.Shared.Dtos.Auth;

using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Shared.Dtos.Auth;
using ESSPortal.Shared.Dtos.Common;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IAuthService 
{
    Task<AppResponse<bool>> RegisterEmployeeAsync(RegisterEmployeeRequest request);
    Task<AppResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<AppResponse<bool>> ResendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<AppResponse<bool>> ConfirmUserEmailAsync(ConfirmUserEmailRequest confirmEmailRequest);

    Task<AppResponse<LoginResponse>> SignInAsync(LoginRequest loginRequest);
    Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync();


    Task<AppResponse<ProviderResponse>> Get2FAProvidersAsync(Get2FAProviderRequest providersRequest);
    Task<AppResponse<Send2FACodeResponse>> Send2FACodeAsync(Send2FACodeRequest sendCodeRequest);
    Task<AppResponse<Verify2FACodeResponse>> Verify2FACodeAsync(Verify2FACodeRequest verifyCodeRequest);


    Task<AppResponse<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<AppResponse<bool>> ValidatePasswordResetTokenAsync(ValidateResetTokenRequest request);
    Task<AppResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AppResponse<bool>> VerifyPasswordAsync(VerifyPasswordRequest verifyPasswordRequest);

    Task<AppResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    Task<AppResponse<bool>> SignOutAsync();


}
