using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.TwoFactor;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ITwoFactorService
{
    Task<ApiResponse<TwoFactorSetupInfo>> GetSetupInfoAsync();
    Task<ApiResponse<TwoFactorStatus>> GetTwoFactorStatusAsync();

    Task<ApiResponse<bool>> EnableTwoFactorAsync(EnableTwoFactorRequest request);
    Task<ApiResponse<bool>> DisableTwoFactorAsync();
    Task<ApiResponse<BackupCodesInfo>> GenerateBackupCodesAsync();
    Task<ApiResponse<bool>> VerifyBackupCodeAsync(VerifyBackupCodeRequest request);
    Task<ApiResponse<Verify2FACodeResponse>> VerifyTotpCodeAsync(VerifyTotpCodeRequest request);
}
