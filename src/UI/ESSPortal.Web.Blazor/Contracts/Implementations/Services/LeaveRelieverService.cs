using Azure.Core;

using EssPortal.Application.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Dtos.Common;
using EssPortal.Web.Blazor.Dtos.ModelFilters;
using EssPortal.Web.Blazor.Models.Navision;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.Services;

internal sealed class LeaveRelieverService : ILeaveRelieverService
{
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<LeaveRelieverService> _logger;
    public LeaveRelieverService(IServiceManager serviceManager, ILogger<LeaveRelieverService> logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> CreateLeaveRelieverAsync(CreateLeaveRelieverRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveRelieversService.CreateAsync(request);

            return apiResponse.Successful
                ? ApiResponse<bool>.Success(apiResponse.Message!, true)
                : ApiResponse<bool>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating leave reliever");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> CreateMultipleLeaveRelieversAsync(List<CreateLeaveRelieverRequest> requests)
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveRelieversService.CreateMultipleAsync(requests);

            return apiResponse.Successful
                ? ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<bool>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating multiple leave relievers");
            throw;
        }
    }

    public async Task<ApiResponse<LeaveRelieverResponse?>> GetLeaveRelieverAsync(string leaveCode, string staffNo)
    {

        try
        {
            var apiResponse = await _serviceManager.LeaveRelieversService.GetLeaveRelieverAsync(leaveCode, staffNo);

            return apiResponse.Successful
                ? ApiResponse<LeaveRelieverResponse?>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<LeaveRelieverResponse?>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving leave reliever");
            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversAsync()
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveRelieversService.GetLeaveRelieversAsync();

            return apiResponse.Successful
                ? ApiResponse<PagedResult<LeaveRelieverResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
                : ApiResponse<PagedResult<LeaveRelieverResponse>>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving leave relievers");
            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo)
    {

        try
        {
            var apiResponse = await _serviceManager.LeaveRelieversService.GetLeaveRelieversByApplicationNoAsync(applicationNo);

            return apiResponse.Successful
                ? ApiResponse<PagedResult<LeaveRelieverResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
                : ApiResponse<PagedResult<LeaveRelieverResponse>>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving leave relievers by application number");
            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<LeaveRelieverResponse>>> SearchLeaveRelieversAsync(LeaveRelieversFilter filter)
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveRelieversService.SearchLeaveRelieversAsync(filter);

            return apiResponse.Successful
                ? ApiResponse<PagedResult<LeaveRelieverResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
                : ApiResponse<PagedResult<LeaveRelieverResponse>>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching leave relievers");
            throw;
        }
    }

    

}
