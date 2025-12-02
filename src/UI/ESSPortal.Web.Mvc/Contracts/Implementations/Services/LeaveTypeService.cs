using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class LeaveTypeService : ILeaveTypeService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;

    public LeaveTypeService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
    }

    // Read operations
    public async Task<AppResponse<List<LeaveTypes>>> GetLeaveTypesAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.GetLeaveTypes;
        return await HandleGetRequest<List<LeaveTypes>>(endpoint);
    }

    public async Task<AppResponse<LeaveTypes?>> GetLeaveTypeByCodeAsync(string code)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.GetLeaveTypeByCode;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "code", code } });
        return await HandleGetRequest<LeaveTypes?>(endpoint);
    }

    public async Task<AppResponse<LeaveTypes?>> GetLeaveTypeByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.GetLeaveTypeByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await HandleGetRequest<LeaveTypes?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveTypes>>> SearchLeaveTypesAsync(LeaveTypeFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.SearchLeaveTypes;
        return await HandlePostRequest<LeaveTypeFilter, List<LeaveTypes>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeaveTypes>> CreateLeaveTypeAsync(LeaveTypes request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.CreateLeaveType;
        return await HandlePostRequest<LeaveTypes, LeaveTypes>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveTypes>>> CreateMultipleLeaveTypesAsync(List<LeaveTypes> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.CreateMultipleLeaveTypes;
        return await HandlePostRequest<List<LeaveTypes>, List<LeaveTypes>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeaveTypes>> UpdateLeaveTypeAsync(LeaveTypes request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.UpdateLeaveType;
        return await HandlePutRequest<LeaveTypes, LeaveTypes>(endpoint, request);
    }

    public async Task<AppResponse<List<LeaveTypes>>> UpdateMultipleLeaveTypesAsync(List<LeaveTypes> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.UpdateMultipleLeaveTypes;
        return await HandlePutRequest<List<LeaveTypes>, List<LeaveTypes>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeaveTypeAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.DeleteLeaveType;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveTypeRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.GetLeaveTypeRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveTypeUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveType.IsLeaveTypeUpdated;
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
