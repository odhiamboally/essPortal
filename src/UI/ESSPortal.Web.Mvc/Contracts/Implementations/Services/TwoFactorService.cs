using EssPortal.Shared.Configurations;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;

using ESSPortal.Shared.Dtos.TwoFactor;
using ESSPortal.Shared.Utilities.Api;

using Microsoft.Extensions.Options;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

namespace ESSPortal.Shared.Contracts.Implementations.Services;

internal sealed class TwoFactorService(
    IServiceManager serviceManager,
    IApiService apiService,
    IOptions<ApiSettings> apiSettings,
    ILogger<TwoFactorService> logger
    
) : ITwoFactorService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;
    private readonly ILogger<TwoFactorService> _logger = logger;

    public async Task<AppResponse<TwoFactorSetupInfo?>> GetSetupInfoAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.GetSetupInfo;
            return await apiService.HandleGetRequest<TwoFactorSetupInfo?>(endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA setup info");
            throw;
        }
    }

    public async Task<AppResponse<TwoFactorStatus?>> GetTwoFactorStatusAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.GetStatus;
            return await apiService.HandleGetRequest<TwoFactorStatus?>(endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status");
            throw;
        }
    }

    public async Task<AppResponse<bool>> EnableTwoFactorAsync(EnableTwoFactorRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.Enable;
            return await apiService.HandlePostRequest<EnableTwoFactorRequest, bool>(endpoint, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA");
            throw;
        }
    }

    public async Task<AppResponse<bool>> DisableTwoFactorAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.Disable;
            return await apiService.HandlePostRequest<object, bool>(endpoint, new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            throw;
        }
    }

    public async Task<AppResponse<BackupCodesInfo?>> GenerateBackupCodesAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.GenerateBackupCodes;
            return await apiService.HandlePostRequest<object, BackupCodesInfo?>(endpoint, new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes");
            throw;
        }
    }

    public async Task<AppResponse<bool>> VerifyTotpCodeAsync(VerifyTotpCodeRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.VerifyTotpCode;
            return await apiService.HandlePostRequest<VerifyTotpCodeRequest, bool>(endpoint, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code");
            throw;
        }
    }


    
}
