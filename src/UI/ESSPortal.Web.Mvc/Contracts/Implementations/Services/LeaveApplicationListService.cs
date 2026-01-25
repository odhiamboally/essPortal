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

namespace ESSPortal.Shared.Contracts.Implementations.Services;

internal sealed class LeaveApplicationListService(
    IServiceManager serviceManager,
    IApiService apiService, 
    IOptions<ApiSettings> apiSettings

) : ILeaveApplicationListService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;


    // Read operations
    public async Task<AppResponse<List<LeaveApplicationListResponse>>> GetLeaveApplicationListsAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.GetLeaveApplicationLists;
        return await apiService.HandleGetRequest<List<LeaveApplicationListResponse>>(endpoint);
    }

    public async Task<AppResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByNoAsync(string applicationNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.GetLeaveApplicationListByNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "applicationNo", applicationNo } });
        return await apiService.HandleGetRequest<LeaveApplicationListResponse?>(endpoint);
    }

    public async Task<AppResponse<LeaveApplicationListResponse?>> GetLeaveApplicationListByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.GetLeaveApplicationListByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await apiService.HandleGetRequest<LeaveApplicationListResponse?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveApplicationListResponse>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.SearchLeaveApplicationLists;
        return await apiService.HandlePostRequest<LeaveApplicationListFilter, List<LeaveApplicationListResponse>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeaveApplicationListResponse>> CreateLeaveApplicationListAsync(CreateLeaveApplicationListRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.CreateLeaveApplicationList;
        return await apiService.HandlePostRequest<CreateLeaveApplicationListRequest, LeaveApplicationListResponse>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveApplicationListResponse>>> CreateMultipleLeaveApplicationListsAsync(List<CreateLeaveApplicationListRequest> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.CreateMultipleLeaveApplicationLists;
        return await apiService.HandlePostRequest<List<CreateLeaveApplicationListRequest>, List<LeaveApplicationListResponse>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeaveApplicationListResponse>> EditLeaveApplicationListAsync(CreateLeaveApplicationListRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.EditLeaveApplicationList;
        return await apiService.HandlePutRequest<CreateLeaveApplicationListRequest, LeaveApplicationListResponse>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveApplicationListResponse>>> UpdateMultipleLeaveApplicationListsAsync(List<CreateLeaveApplicationListRequest> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.UpdateMultipleLeaveApplicationLists;
        return await apiService.HandlePutRequest<List<CreateLeaveApplicationListRequest>, List<LeaveApplicationListResponse>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeaveApplicationListAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.DeleteLeaveApplicationList;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveApplicationListRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.GetLeaveApplicationListRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveApplicationListUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.IsLeaveApplicationListUpdated;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<bool>(endpoint);
    }

   
}
