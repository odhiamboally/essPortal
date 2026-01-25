using EssPortal.Shared.Configurations;

using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Dashboard;
using ESSPortal.Shared.Utilities.Api;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

using Microsoft.Extensions.Options;

namespace ESSPortal.Shared.Contracts.Implementations.Services;

public class DashboardService(
    
    IApiService apiService, 
    IOptions<ApiSettings> apiSettings

) : IDashboardService
{
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    public async Task<AppResponse<DashboardResponse>> GetDashboardDataAsync(string employeeNo)
    {
        try
        {
            
            var endpoint = _apiSettings.ApiEndpoints?.Dashboard?.GetDashboardData;
            endpoint = EndpointHelper.ReplaceVersion(endpoint, _apiSettings.Version);
            endpoint = EndpointHelper.ReplaceParams(endpoint, new() { { "employeeNo", employeeNo } });

            var apiResponse = await apiService.GetAsync<DashboardResponse>(endpoint);

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
