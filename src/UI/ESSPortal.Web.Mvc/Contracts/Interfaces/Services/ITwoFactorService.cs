using EssPortal.Web.Mvc.Dtos.Common;

using ESSPortal.Web.Mvc.Dtos.TwoFactor;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ITwoFactorService
{
    Task<AppResponse<TwoFactorSetupInfo?>> GetSetupInfoAsync();
    Task<AppResponse<TwoFactorStatus?>> GetTwoFactorStatusAsync();
    Task<AppResponse<bool>> EnableTwoFactorAsync(EnableTwoFactorRequest request);
    Task<AppResponse<bool>> DisableTwoFactorAsync();
    Task<AppResponse<BackupCodesInfo?>> GenerateBackupCodesAsync();
    Task<AppResponse<bool>> VerifyTotpCodeAsync(VerifyTotpCodeRequest request);



}
