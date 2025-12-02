using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Dtos.ModelFilters;
using EssPortal.Web.Mvc.Models.Navision;

namespace ESSPortal.Web.Mvc.Contracts.Interfaces.Services;

public interface ILeaveStatisticsFactboxService
{
    // Read operations (Statistics are typically read-only)
    Task<AppResponse<List<LeaveStatisticsFactbox>>> GetLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter);
    Task<AppResponse<LeaveStatisticsFactbox?>> GetLeaveStatsByRecIdAsync(string recId);
    Task<AppResponse<List<LeaveStatisticsFactbox>>> SearchLeaveStatisticsAsync(LeaveStatisticsFactboxFilter filter);

    // Utility operations
    Task<AppResponse<string?>> GetLeaveStatsRecIdFromKeyAsync(string key);
    Task<AppResponse<bool>> IsLeaveStatsUpdatedAsync(string key);
}
