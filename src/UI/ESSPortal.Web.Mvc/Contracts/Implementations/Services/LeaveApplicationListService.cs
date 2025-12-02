using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class LeaveApplicationListService : ILeaveApplicationListService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;

    public LeaveApplicationListService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
    }


    // Read operations
    public async Task<AppResponse<List<LeaveApplicationList>>> GetLeaveApplicationListsAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.GetLeaveApplicationLists;
        return await HandleGetRequest<List<LeaveApplicationList>>(endpoint);
    }

    public async Task<AppResponse<LeaveApplicationList?>> GetLeaveApplicationListByNoAsync(string applicationNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.GetLeaveApplicationListByNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "applicationNo", applicationNo } });
        return await HandleGetRequest<LeaveApplicationList?>(endpoint);
    }

    public async Task<AppResponse<LeaveApplicationList?>> GetLeaveApplicationListByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.GetLeaveApplicationListByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await HandleGetRequest<LeaveApplicationList?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveApplicationList>>> SearchLeaveApplicationListsAsync(LeaveApplicationListFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.SearchLeaveApplicationLists;
        return await HandlePostRequest<LeaveApplicationListFilter, List<LeaveApplicationList>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeaveApplicationList>> CreateLeaveApplicationListAsync(LeaveApplicationList request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.CreateLeaveApplicationList;
        return await HandlePostRequest<LeaveApplicationList, LeaveApplicationList>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveApplicationList>>> CreateMultipleLeaveApplicationListsAsync(List<LeaveApplicationList> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.CreateMultipleLeaveApplicationLists;
        return await HandlePostRequest<List<LeaveApplicationList>, List<LeaveApplicationList>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeaveApplicationList>> UpdateLeaveApplicationListAsync(LeaveApplicationList request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.UpdateLeaveApplicationList;
        return await HandlePutRequest<LeaveApplicationList, LeaveApplicationList>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveApplicationList>>> UpdateMultipleLeaveApplicationListsAsync(List<LeaveApplicationList> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.UpdateMultipleLeaveApplicationLists;
        return await HandlePutRequest<List<LeaveApplicationList>, List<LeaveApplicationList>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeaveApplicationListAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.DeleteLeaveApplicationList;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveApplicationListRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.GetLeaveApplicationListRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveApplicationListUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveApplicationList.IsLeaveApplicationListUpdated;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<bool>(endpoint);
    }

    // Helper methods
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

    private async Task<AppResponse<TResponse>> HandlePutRequest<TRequest, TResponse>(string endpoint, TRequest request)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return AppResponse<TResponse>.Failure("Endpoint not configured.");

        endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
        var apiResponse = await _apiService.PutAsync<TRequest, TResponse>(endpoint, request);

        return apiResponse.Successful
            ? AppResponse<TResponse>.Success(apiResponse.Message!, apiResponse.Data!)
            : AppResponse<TResponse>.Failure(apiResponse.Message!);
    }

    private async Task<AppResponse<T>> HandleDeleteRequest<T>(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return AppResponse<T>.Failure("Endpoint not configured.");

        endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
        var apiResponse = await _apiService.DeleteAsync<T>(endpoint);

        return apiResponse.Successful
            ? AppResponse<T>.Success(apiResponse.Message!, apiResponse.Data!)
            : AppResponse<T>.Failure(apiResponse.Message!);
    }
}
