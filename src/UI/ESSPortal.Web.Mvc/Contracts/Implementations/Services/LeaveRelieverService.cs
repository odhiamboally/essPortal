using EssPortal.Shared.Configurations;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;

using ESSPortal.Shared.Utilities.Api;

using Microsoft.Extensions.Options;
using ESSPortal.Shared.Dtos.Leave;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using EssPortal.Shared.Dtos.Leave;

namespace ESSPortal.Shared.Contracts.Implementations.Services;

internal sealed class LeaveRelieverService(
    IServiceManager serviceManager,
    IApiService apiService, 
    IOptions<ApiSettings> apiSettings

) : ILeaveRelieverService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    // Read operations
    public async Task<AppResponse<List<LeaveRelieverResponse>>> GetLeaveRelieversAsync(LeaveRelieverFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelievers;
        return await apiService.HandlePostRequest<LeaveRelieverFilter, List<LeaveRelieverResponse>>(endpoint, filter);
    }

    public async Task<AppResponse<LeaveRelieverResponse?>> GetLeaveRelieverAsync(string leaveCode, string staffNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelieverByComposite;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new()
        {
            { "leaveCode", leaveCode },
            { "staffNo", staffNo }
        });
        return await apiService.HandleGetRequest<LeaveRelieverResponse?>(endpoint);
    }

    public async Task<AppResponse<LeaveRelieverResponse?>> GetLeaveRelieverByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelieverByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await apiService.HandleGetRequest<LeaveRelieverResponse?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveRelieverResponse>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelieversByApplicationNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "applicationNo", applicationNo } });
        return await apiService.HandleGetRequest<List<LeaveRelieverResponse>>(endpoint);
    }

    public async Task<AppResponse<List<LeaveRelieverResponse>>> SearchLeaveRelieversAsync(LeaveRelieverFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.SearchLeaveRelievers;
        return await apiService.HandlePostRequest<LeaveRelieverFilter, List<LeaveRelieverResponse>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeaveRelieverResponse>> CreateLeaveRelieverAsync(CreateLeaveRelieverRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.CreateLeaveReliever;
        return await apiService.HandlePostRequest<CreateLeaveRelieverRequest, LeaveRelieverResponse>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveRelieverResponse>>> CreateMultipleLeaveRelieversAsync(List<CreateLeaveRelieverRequest> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.CreateMultipleLeaveRelievers;
        return await apiService.HandlePostRequest<List<CreateLeaveRelieverRequest>, List<LeaveRelieverResponse>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeaveRelieverResponse>> UpdateLeaveRelieverAsync(CreateLeaveRelieverRequest request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.UpdateLeaveReliever;
        return await apiService.HandlePutRequest<CreateLeaveRelieverRequest, LeaveRelieverResponse>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveRelieverResponse>>> UpdateMultipleLeaveRelieversAsync(List<CreateLeaveRelieverRequest> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.UpdateMultipleLeaveRelievers;
        return await apiService.HandlePutRequest<List<CreateLeaveRelieverRequest>, List<LeaveRelieverResponse>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeaveRelieverAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.DeleteLeaveReliever;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveRelieverRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelieverRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveRelieverUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.IsLeaveRelieverUpdated;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<bool>(endpoint);
    }

    
    

}
