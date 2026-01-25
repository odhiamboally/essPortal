using EssPortal.Shared.Dtos.Auth;

using ESSPortal.Application.Dtos.Auth;

using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.TwoFactor;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface ITwoFactorService
{
    Task<AppResponse<TwoFactorSetupInfo>> GetSetupInfoAsync();
    Task<AppResponse<TwoFactorStatus>> GetTwoFactorStatusAsync();

    Task<AppResponse<bool>> EnableTwoFactorAsync(EnableTwoFactorRequest request);
    Task<AppResponse<bool>> DisableTwoFactorAsync();
    Task<AppResponse<BackupCodesInfo>> GenerateBackupCodesAsync();
    Task<AppResponse<bool>> VerifyBackupCodeAsync(VerifyBackupCodeRequest request);
    Task<AppResponse<Verify2FACodeResponse>> VerifyTotpCodeAsync(VerifyTotpCodeRequest request);
}
