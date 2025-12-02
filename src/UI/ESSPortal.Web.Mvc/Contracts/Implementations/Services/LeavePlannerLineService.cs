using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class LeavePlannerLineService : ILeavePlannerLineService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;

    public LeavePlannerLineService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
    }

    // Read operations
    public async Task<AppResponse<List<LeavePlannerLine>>> GetLeavePlannerLinesAsync()
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.GetLeavePlannerLines;
        return await HandleGetRequest<List<LeavePlannerLine>>(endpoint);
    }

    public async Task<AppResponse<LeavePlannerLine?>> GetLeavePlannerLineAsync(string employeeNo, string leaveType)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.GetLeavePlannerLineByComposite;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new()
        {
            { "employeeNo", employeeNo },
            { "leaveType", leaveType }
        });
        return await HandleGetRequest<LeavePlannerLine?>(endpoint);
    }

    public async Task<AppResponse<LeavePlannerLine?>> GetLeavePlannerLineByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.GetLeavePlannerLineByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await HandleGetRequest<LeavePlannerLine?>(endpoint);
    }

    public async Task<AppResponse<List<LeavePlannerLine>>> SearchLeavePlannerLinesAsync(LeavePlannerLinesFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.SearchLeavePlannerLines;
        return await HandlePostRequest<LeavePlannerLinesFilter, List<LeavePlannerLine>>(endpoint, filter);
    }

    // Create operations
    public async Task<AppResponse<LeavePlannerLine>> CreateLeavePlannerLineAsync(LeavePlannerLine request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.CreateLeavePlannerLine;
        return await HandlePostRequest<LeavePlannerLine, LeavePlannerLine>(endpoint, request);
    }

    public async Task<AppResponse<List<LeavePlannerLine>>> CreateMultipleLeavePlannerLinesAsync(List<LeavePlannerLine> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.CreateMultipleLeavePlannerLines;
        return await HandlePostRequest<List<LeavePlannerLine>, List<LeavePlannerLine>>(endpoint, requests);
    }

    // Update operations
    public async Task<AppResponse<LeavePlannerLine>> UpdateLeavePlannerLineAsync(LeavePlannerLine request)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.UpdateLeavePlannerLine;
        return await HandlePutRequest<LeavePlannerLine, LeavePlannerLine>(endpoint, request);
    }

    public async Task<AppResponse<List<LeavePlannerLine>>> UpdateMultipleLeavePlannerLinesAsync(List<LeavePlannerLine> requests)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.UpdateMultipleLeavePlannerLines;
        return await HandlePutRequest<List<LeavePlannerLine>, List<LeavePlannerLine>>(endpoint, requests);
    }

    // Delete operations
    public async Task<AppResponse<bool>> DeleteLeavePlannerLineAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.DeleteLeavePlannerLine;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleDeleteRequest<bool>(endpoint);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeavePlannerLineRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.GetLeavePlannerLineRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeavePlannerLineUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeavePlannerLine.IsLeavePlannerLineUpdated;
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
