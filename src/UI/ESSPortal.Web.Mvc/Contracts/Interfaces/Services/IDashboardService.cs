using EssPortal.Web.Mvc.Dtos.Common;

using ESSPortal.Web.Mvc.Dtos.Dashboard;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface IDashboardService
{
    Task<AppResponse<DashboardResponse>> GetDashboardDataAsync(string employeeNo);
}
