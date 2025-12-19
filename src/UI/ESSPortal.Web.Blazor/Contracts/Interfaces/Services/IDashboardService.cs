using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Dashboard;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Services;

public interface IDashboardService
{
    Task<ApiResponse<DashboardResponse>> GetDashboardDataAsync(string employeeNo);
}
