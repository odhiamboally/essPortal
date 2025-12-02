using ESSPortal.Application.Dtos.Common;
using ESSPortal.Application.Dtos.Dashboard;

namespace ESSPortal.Application.Contracts.Interfaces.Services;
public interface IDashboardService
{
    Task<ApiResponse<DashboardResponse>> GetDashboardDataAsync(string employeeNo);
}
