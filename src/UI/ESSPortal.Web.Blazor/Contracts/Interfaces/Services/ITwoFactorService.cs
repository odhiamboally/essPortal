using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.TwoFactor;
using ESSPortal.Web.Blazor.ViewModels.Auth;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface ITwoFactorService
{
    Task<ApiResponse<TwoFactorSetupInfo?>> GetSetupInfoAsync();
    Task<ApiResponse<TwoFactorStatus?>> GetTwoFactorStatusAsync();
    Task<ApiResponse<bool>> EnableTwoFactorAsync(EnableTwoFactorRequest request);
    Task<ApiResponse<bool>> DisableTwoFactorAsync();
    Task<ApiResponse<BackupCodesInfo?>> GenerateBackupCodesAsync();
    Task<ApiResponse<bool>> VerifyTotpCodeAsync(VerifyTotpCodeViewModel viewModel);



}
