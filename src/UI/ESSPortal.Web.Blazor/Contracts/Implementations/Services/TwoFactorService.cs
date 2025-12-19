using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Application.Dtos.TwoFactor;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Utilities.Api;
using ESSPortal.Web.Blazor.ViewModels.Auth;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.AppServices;

internal sealed class TwoFactorService : ITwoFactorService
{
    private readonly ILogger<TwoFactorService> _logger;
    private readonly IServiceManager _serviceManager;
    public TwoFactorService(ILogger<TwoFactorService> logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }


    public async Task<ApiResponse<TwoFactorSetupInfo?>> GetSetupInfoAsync()
    {
        try
        {
            var apiResponse = await _serviceManager.TwoFactorService.GetSetupInfoAsync();

            return apiResponse.Successful
                ? ApiResponse<TwoFactorSetupInfo?>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<TwoFactorSetupInfo?>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA setup info");
            throw;
        }
    }

    public async Task<ApiResponse<TwoFactorStatus?>> GetTwoFactorStatusAsync()
    {
        try
        {
            var apiResponse = await _serviceManager.TwoFactorService.GetTwoFactorStatusAsync();

            return apiResponse.Successful
                ? ApiResponse<TwoFactorStatus?>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<TwoFactorStatus?>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> EnableTwoFactorAsync(EnableTwoFactorRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.TwoFactorService.EnableTwoFactorAsync(request);

            return apiResponse.Successful
                ? ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<bool>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> DisableTwoFactorAsync()
    {
        try
        {
            var apiResponse = await _serviceManager.TwoFactorService.DisableTwoFactorAsync();

            return apiResponse.Successful
                ? ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<bool>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            throw;
        }
    }

    public async Task<ApiResponse<BackupCodesInfo?>> GenerateBackupCodesAsync()
    {
        try
        {
            var apiResponse = await _serviceManager.TwoFactorService.GenerateBackupCodesAsync();

            return apiResponse.Successful
                ? ApiResponse<BackupCodesInfo?>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<BackupCodesInfo?>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> VerifyTotpCodeAsync(VerifyTotpCodeViewModel viewModel)
    {
        try
        {
            var request = new VerifyTotpCodeRequest
            {
                UserId = viewModel.UserId,
                Code = viewModel.Code
            };

            var apiResponse = await _serviceManager.TwoFactorService.VerifyTotpCodeAsync(request);

            return apiResponse.Successful
                ? ApiResponse<bool>.Success(apiResponse.Message!, true)
                : ApiResponse<bool>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code");
            throw;
        }
    }



}
