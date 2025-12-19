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

internal sealed class LeaveTypeService : ILeaveTypeService
{
    private readonly ILogger<LeaveTypeService> _logger;
    private readonly IServiceManager _serviceManager;


    public LeaveTypeService(ILogger<LeaveTypeService> logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }

    public async Task<ApiResponse<bool>> CreateLeaveTypeAsync(CreateLeaveTypeRequest request)
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveTypeService.CreateLeaveTypeAsync(request);

            return apiResponse.Successful
                ? ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<bool>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating leave type");

            throw;
        }
       
    }

    public async Task<ApiResponse<PagedResult<LeaveTypeResponse>>> GetLeaveTypesAsync()
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveTypeService.GetLeaveTypesAsync();

            return apiResponse.Successful
                ? ApiResponse<PagedResult<LeaveTypeResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
                : ApiResponse<PagedResult<LeaveTypeResponse>>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching leave types");
            throw;
        }
        
    }

    public async Task<ApiResponse<LeaveTypeResponse?>> GetLeaveTypeByCodeAsync(string code)
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveTypeService.GetLeaveTypeByCodeAsync(code);

            return apiResponse.Successful
                ? ApiResponse<LeaveTypeResponse?>.Success(apiResponse.Message!, apiResponse.Data)
                : ApiResponse<LeaveTypeResponse?>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching leave type by code");
            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<LeaveTypeResponse>>> SearchLeaveTypesAsync(LeaveTypeFilter filter)
    {
        try
        {
            var apiResponse = await _serviceManager.LeaveTypeService.SearchLeaveTypesAsync(filter);

            return apiResponse.Successful
                ? ApiResponse<PagedResult<LeaveTypeResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
                : ApiResponse<PagedResult<LeaveTypeResponse>>.Failure(apiResponse.Message!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching leave types");

            throw;
        }
        
    }

}
