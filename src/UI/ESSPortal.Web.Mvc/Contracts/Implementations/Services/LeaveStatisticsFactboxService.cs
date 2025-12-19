using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Utilities.Api;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Services;

internal sealed class LeaveStatisticsFactboxService : ILeaveStatisticsFactboxService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;

    public LeaveStatisticsFactboxService(IApiService apiService, IOptions<ApiSettings> apiSettings)
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;
    }


    // Read operations
    public async Task<AppResponse<List<LeaveStatisticsFactbox>>> GetLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.GetLeaveStatistics;
        return await HandlePostRequest<LeaveStatisticsFactboxFilter, List<LeaveStatisticsFactbox>>(endpoint, filter);
    }

    public async Task<AppResponse<LeaveStatisticsFactbox?>> GetLeaveStatsByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.GetLeaveStatsByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await HandleGetRequest<LeaveStatisticsFactbox?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveStatisticsFactbox>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.SearchLeaveStatistics;
        return await HandlePostRequest<LeaveStatisticsFactboxFilter, List<LeaveStatisticsFactbox>>(endpoint, filter);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveStatsRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.GetLeaveStatsRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveStatsUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.IsLeaveStatsUpdated;
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
}
