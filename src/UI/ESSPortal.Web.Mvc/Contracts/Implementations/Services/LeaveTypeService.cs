using EssPortal.Shared.Configurations;
using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Leave;
using ESSPortal.Shared.Utilities.Api;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class LeaveTypeService(
    IServiceManager serviceManager,
    IApiService apiService, 
    IOptions<ApiSettings> apiSettings

) : ILeaveTypeService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    // Read operations
    public async Task<AppResponse<List<LeaveTypeResponse>>> GetLeaveTypesAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.GetLeaveTypes;
        return await apiService.HandleGetRequest<List<LeaveTypeResponse>>(endpoint);
    }

    public async Task<AppResponse<LeaveTypeResponse?>> GetLeaveTypeByCodeAsync(string code)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.GetLeaveTypeByCode;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "code", code } });
        return await apiService.HandleGetRequest<LeaveTypeResponse?>(endpoint);
    }

    public async Task<AppResponse<LeaveTypeResponse?>> GetLeaveTypeByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.GetLeaveTypeByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await apiService.HandleGetRequest<LeaveTypeResponse?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveTypeResponse>>> SearchLeaveTypesAsync(LeaveTypeFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.SearchLeaveTypes;
        return await apiService.HandlePostRequest<LeaveTypeFilter, List<LeaveTypeResponse>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeaveTypeResponse>> CreateLeaveTypeAsync(CreateLeaveTypeRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.CreateLeaveType;
        return await apiService.HandlePostRequest<CreateLeaveTypeRequest, LeaveTypeResponse>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveTypeResponse>>> CreateMultipleLeaveTypesAsync(List<CreateLeaveTypeRequest> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.CreateMultipleLeaveTypes;
        return await apiService.HandlePostRequest<List<CreateLeaveTypeRequest>, List<LeaveTypeResponse>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeaveTypeResponse>> UpdateLeaveTypeAsync(CreateLeaveTypeRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.UpdateLeaveType;
        return await apiService.HandlePutRequest<CreateLeaveTypeRequest, LeaveTypeResponse>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveTypeResponse>>> UpdateMultipleLeaveTypesAsync(List<CreateLeaveTypeRequest> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.UpdateMultipleLeaveTypes;
        return await apiService.HandlePutRequest<List<CreateLeaveTypeRequest>, List<LeaveTypeResponse>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeaveTypeAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.DeleteLeaveType;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveTypeRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.GetLeaveTypeRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveTypeUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.IsLeaveTypeUpdated;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<bool>(endpoint);
    }

    

    
}
