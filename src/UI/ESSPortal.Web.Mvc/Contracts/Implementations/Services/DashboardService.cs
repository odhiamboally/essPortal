using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Dtos.Dashboard;
using ESSPortal.Web.Mvc.Utilities.Api;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.AppServices;

public class DashboardService : IDashboardService
{
    private readonly IApiService _apiService;
    private readonly ApiSettings _apiSettings;


    public DashboardService(IApiService apiService, IOptions<ApiSettings> apiSettings )
    {
        _apiService = apiService;
        _apiSettings = apiSettings.Value;

    }

    public async Task<AppResponse<DashboardResponse>> GetDashboardDataAsync(string employeeNo)
    {
        try
        {
            
            var endpoint = _apiSettings.ApiEndpoints?.Dashboard?.GetDashboardData;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
            endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });

            var apiResponse = await _apiService.GetAsync<DashboardResponse>(endpoint);

            return apiResponse.Successful
                ? AppResponse<DashboardResponse>.Success(apiResponse.Message!, apiResponse.Data!)
                : AppResponse<DashboardResponse>.Failure(apiResponse.Message!);
        }
        catch (Exception)
        {

            throw;
        }
    }
}
