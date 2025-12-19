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

internal sealed class LeaveApplicationListService : ILeaveApplicationListService
{
    private readonly IServiceManager _serviceManager;
    public LeaveApplicationListService(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<ApiResponse<bool>> CreateLeaveApplicationListAsync(CreateLeaveApplicationListRequest request)
    {
        var apiResponse = await _serviceManager.LeaveApplicationListService.CreateLeaveApplicationListAsync(request);

        return apiResponse.Successful
            ? ApiResponse<bool>.Success(apiResponse.Message!, apiResponse.Data)
            : ApiResponse<bool>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationListResponse>>> GetLeaveApplicationListsAsync()
    {
        var apiResponse = await _serviceManager.LeaveApplicationListService.GetLeaveApplicationListsAsync();

        return apiResponse.Successful
            ? ApiResponse<PagedResult<LeaveApplicationListResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
            : ApiResponse<PagedResult<LeaveApplicationListResponse>>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByNoAsync(string applicationNo)
    {
        var apiResponse = await _serviceManager.LeaveApplicationListService.GetLeaveApplicationListByNoAsync(applicationNo);

        return apiResponse.Successful
            ? ApiResponse<LeaveApplicationListResponse?>.Success(apiResponse.Message!, apiResponse.Data)
            : ApiResponse<LeaveApplicationListResponse?>.Failure(apiResponse.Message!);
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationListResponse>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter)
    {
        var apiResponse = await _serviceManager.LeaveApplicationListService.SearchLeaveApplicationListsAsync(filter);
        return apiResponse.Successful
            ? ApiResponse<PagedResult<LeaveApplicationListResponse>>.Success(apiResponse.Message!, apiResponse.Data ?? new())
            : ApiResponse<PagedResult<LeaveApplicationListResponse>>.Failure(apiResponse.Message!);
    }

    
}
