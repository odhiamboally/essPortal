using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Models.Common;
using ESSPortal.Web.Mvc.Utilities.Session;

namespace ESSPortal.Web.Mvc.Extensions;

public static class CacheStatisticsExtensions
{
    public static CacheStatistics GetCacheStatistics(this ICacheService cache, string employeeNo)
    {
        return new CacheStatistics
        {
            EmployeeNo = employeeNo,
            HasDashboardCache = cache.Exists(CacheKeys.Dashboard(employeeNo)),
            HasLeaveHistoryCache = cache.Exists(CacheKeys.LeaveHistory(employeeNo)),
            HasLeaveSummaryCache = cache.Exists(CacheKeys.LeaveSummary(employeeNo)),
            HasLeaveTypesCache = cache.Exists(CacheKeys.LeaveTypes()),
            Timestamp = DateTime.UtcNow
        };
    }
}
