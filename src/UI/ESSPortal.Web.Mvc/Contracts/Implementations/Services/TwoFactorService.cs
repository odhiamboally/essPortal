using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Dtos.TwoFactor;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.AppServices;

internal sealed class TwoFactorService : ITwoFactorService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<TwoFactorService> _logger;

    public TwoFactorService(
        IApiService apiService,
        IOptions<ApiSettings> apiSettings,
        ILogger<TwoFactorService> logger)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }


    public async Task<AppResponse<TwoFactorSetupInfo?>> GetSetupInfoAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.GetSetupInfo;
            return await HandleGetRequest<TwoFactorSetupInfo?>(endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA setup info");
            return AppResponse<TwoFactorSetupInfo?>.Failure("An error occurred while getting setup information.");
        }
    }

    public async Task<AppResponse<TwoFactorStatus?>> GetTwoFactorStatusAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.GetStatus;
            return await HandleGetRequest<TwoFactorStatus?>(endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status");
            return AppResponse<TwoFactorStatus?>.Failure("An error occurred while getting two-factor status.");
        }
    }

    public async Task<AppResponse<bool>> EnableTwoFactorAsync(EnableTwoFactorRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.Enable;
            return await HandlePostRequest<EnableTwoFactorRequest, bool>(endpoint, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA");
            return AppResponse<bool>.Failure("An error occurred while enabling two-factor authentication.");
        }
    }

    public async Task<AppResponse<bool>> DisableTwoFactorAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.Disable;
            return await HandlePostRequest<object, bool>(endpoint, new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            return AppResponse<bool>.Failure("An error occurred while disabling two-factor authentication.");
        }
    }

    public async Task<AppResponse<BackupCodesInfo?>> GenerateBackupCodesAsync()
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.GenerateBackupCodes;
            return await HandlePostRequest<object, BackupCodesInfo?>(endpoint, new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes");
            return AppResponse<BackupCodesInfo?>.Failure("An error occurred while generating backup codes.");
        }
    }

    public async Task<AppResponse<bool>> VerifyTotpCodeAsync(VerifyTotpCodeRequest request)
    {
        try
        {
            var endpoint = _apiSettings.ApiEndpoints.TwoFactor.VerifyTotpCode;
            return await HandlePostRequest<VerifyTotpCodeRequest, bool>(endpoint, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code");
            return AppResponse<bool>.Failure("An error occurred while verifying the code.");
        }
    }



    private async Task<AppResponse<T>> HandleGetRequest<T>(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return AppResponse<T>.Failure("Endpoint not configured.");

        endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
        var apiResponse = await _apiService.GetAsync<T>(endpoint);

        return apiResponse.Successful
            ? AppResponse<T>.Success(apiResponse.Message!, apiResponse.Data!)
            : AppResponse<T>.Failure(apiResponse.Message!);
    }

    private async Task<AppResponse<TResponse>> HandlePostRequest<TRequest, TResponse>(string endpoint, TRequest request)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return AppResponse<TResponse>.Failure("Endpoint not configured.");

        endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
        var apiResponse = await _apiService.PostAsync<TRequest, TResponse>(endpoint, request);

        return apiResponse.Successful
            ? AppResponse<TResponse>.Success(apiResponse.Message!, apiResponse.Data!)
            : AppResponse<TResponse>.Failure(apiResponse.Message!);
    }

    
}
