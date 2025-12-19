using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Dashboard;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Common;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Utilities.Api;
using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.AppServices;

public class DashboardService : IDashboardService
{

    private readonly IServiceManager _serviceManager;

    public DashboardService(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<ApiResponse<DashboardResponse>> GetDashboardDataAsync(string employeeNo)
    {
        try
        {
            var apiResponse = await _serviceManager.DashboardService.GetDashboardDataAsync(employeeNo);

            return apiResponse.Successful
                ? ApiResponse<DashboardResponse>.Success(apiResponse.Message!, apiResponse.Data!)
                : ApiResponse<DashboardResponse>.Failure(apiResponse.Message!);
        }
        catch (Exception)
        {

            throw;
        }
    }
}
