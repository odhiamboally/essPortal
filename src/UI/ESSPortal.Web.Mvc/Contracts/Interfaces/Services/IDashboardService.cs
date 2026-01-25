
using ESSPortal.Shared.Dtos.Common;
using ESSPortal.Shared.Dtos.Dashboard;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IDashboardService
{
    Task<AppResponse<DashboardResponse>> GetDashboardDataAsync(string employeeNo);
}
