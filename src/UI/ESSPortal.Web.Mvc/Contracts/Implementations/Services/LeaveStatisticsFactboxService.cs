using EssPortal.Shared.Configurations;
using EssPortal.Shared.Dtos.Leave;
using EssPortal.Shared.Dtos.ModelFilters;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Utilities.Api;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.Extensions.Options;

namespace ESSPortal.Shared.Contracts.Implementations.Services;

internal sealed class LeaveStatisticsFactboxService(
    IServiceManager serviceManager,
    IApiService apiService, 
    IOptions<ApiSettings> apiSettings

) : ILeaveStatisticsFactboxService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;


    // Read operations
    public async Task<AppResponse<List<LeaveStatisticsFactboxResponse>>> GetLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.GetLeaveStatistics;
        return await apiService.HandlePostRequest<LeaveStatisticsFactboxFilter, List<LeaveStatisticsFactboxResponse>>(endpoint, filter);
    }

    public async Task<AppResponse<LeaveStatisticsFactboxResponse?>> GetLeaveStatsByRecIdAsync(string recId)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.GetLeaveStatsByRecId;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "recId", recId } });
        return await apiService.HandleGetRequest<LeaveStatisticsFactboxResponse?>(endpoint);
    }

    public async Task<AppResponse<List<LeaveStatisticsFactboxResponse>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.SearchLeaveStatistics;
        return await apiService.HandlePostRequest<LeaveStatisticsFactboxFilter, List<LeaveStatisticsFactboxResponse>>(endpoint, filter);
    }

    // Utility operations
    public async Task<AppResponse<string?>> GetLeaveStatsRecIdFromKeyAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.GetLeaveStatsRecIdFromKey;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<string?>(endpoint);
    }

    public async Task<AppResponse<bool>> IsLeaveStatsUpdatedAsync(string key)
    {
        var endpoint = _apiSettings.ApiEndpoints.LeaveStatisticsFactbox.IsLeaveStatsUpdated;
        endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "key", key } });
        return await apiService.HandleGetRequest<bool>(endpoint);
    }
}

    
