using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class LeaveRelieverService : ILeaveRelieverService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;

    public LeaveRelieverService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
    }

    // Read operations
    public async Task<AppResponse<List<LeaveReliever>>> GetLeaveRelieversAsync(LeaveRelieverFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelievers;
        return await HandlePostRequest<LeaveRelieverFilter, List<LeaveReliever>>(endpoint, filter);
    }

    public async Task<AppResponse<LeaveReliever?>> GetLeaveRelieverAsync(string leaveCode, string staffNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelieverByComposite;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new()
        {
            { "leaveCode", leaveCode },
            { "staffNo", staffNo }
        });
        return await HandleGetRequest<LeaveReliever?>(endpoint);
    }

    public async Task<AppResponse<LeaveReliever?>> GetLeaveRelieverByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelieverByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await HandleGetRequest<LeaveReliever?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveReliever>>> GetLeaveRelieversByApplicationNoAsync(string applicationNo)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelieversByApplicationNo;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "applicationNo", applicationNo } });
        return await HandleGetRequest<List<LeaveReliever>>(endpoint);
    }

    public async Task<AppResponse<List<LeaveReliever>>> SearchLeaveRelieversAsync(LeaveRelieverFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.SearchLeaveRelievers;
        return await HandlePostRequest<LeaveRelieverFilter, List<LeaveReliever>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeaveReliever>> CreateLeaveRelieverAsync(LeaveReliever request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.CreateLeaveReliever;
        return await HandlePostRequest<LeaveReliever, LeaveReliever>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveReliever>>> CreateMultipleLeaveRelieversAsync(List<LeaveReliever> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.CreateMultipleLeaveRelievers;
        return await HandlePostRequest<List<LeaveReliever>, List<LeaveReliever>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeaveReliever>> UpdateLeaveRelieverAsync(LeaveReliever request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.UpdateLeaveReliever;
        return await HandlePutRequest<LeaveReliever, LeaveReliever>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveReliever>>> UpdateMultipleLeaveRelieversAsync(List<LeaveReliever> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.UpdateMultipleLeaveRelievers;
        return await HandlePutRequest<List<LeaveReliever>, List<LeaveReliever>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeaveRelieverAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.DeleteLeaveReliever;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveRelieverRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.GetLeaveRelieverRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveRelieverUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveReliever.IsLeaveRelieverUpdated;
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
