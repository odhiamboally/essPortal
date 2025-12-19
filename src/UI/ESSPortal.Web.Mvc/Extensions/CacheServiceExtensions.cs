using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Dtos.Dashboard;
using ESSPortal.Web.Mvc.Dtos.Leave;
using ESSPortal.Web.Mvc.Utilities.Session;

namespace ESSPortal.Web.Mvc.Extensions;

public static class CacheServiceExtensions
{
    // Session methods
    public static string? GetSessionId(this ICacheService cache, string employeeNo)
    {
        return cache.Get<string>(CacheKeys.SessionId(employeeNo));
    }

    public static void SetSessionId(this ICacheService cache, string employeeNo, string data)
    {
        cache.Set(CacheKeys.SessionId(employeeNo), data, CacheExpiration.SessionId);
    }

    // UserInfo methods
    public static UserInfo? GetUserInfo(this ICacheService cache, string employeeNo)
    {
        return cache.Get<UserInfo>(CacheKeys.UserInfo(employeeNo));
    }

    public static void SetUserInfo(this ICacheService cache, string employeeNo, UserInfo data)
    {
        cache.Set(CacheKeys.UserInfo(employeeNo), data, CacheExpiration.UserInfo);
    }

    // Dashboard methods
    public static DashboardResponse? GetDashboard(this ICacheService cache, string employeeNo)
    {
        return cache.Get<DashboardResponse>(CacheKeys.Dashboard(employeeNo));
    }

    public static void SetDashboard(this ICacheService cache, string employeeNo, DashboardResponse data)
    {
        cache.Set(CacheKeys.Dashboard(employeeNo), data, CacheExpiration.Dashboard);
    }

    public static void InvalidateDashboard(this ICacheService cache, string employeeNo)
    {
        cache.Remove(CacheKeys.Dashboard(employeeNo));
    }

    // Leave methods
    public static List<LeaveTypeResponse>? GetLeaveTypes(this ICacheService cache)
    {
        return cache.Get<List<LeaveTypeResponse>>(CacheKeys.LeaveTypes());
    }

    public static void SetLeaveTypes(this ICacheService cache, List<LeaveTypeResponse> data)
    {
        cache.Set(CacheKeys.LeaveTypes(), data, CacheExpiration.LeaveTypes);
    }

    public static void InvalidateLeaveTypes(this ICacheService cache)
    {
        cache.Remove(CacheKeys.LeaveTypes());
    }

    public static List<LeaveHistoryResponse>? GetLeaveHistory(this ICacheService cache, string employeeNo)
    {
        return cache.Get<List<LeaveHistoryResponse>>(CacheKeys.LeaveHistory(employeeNo));
    }

    public static void SetLeaveHistory(this ICacheService cache, string employeeNo, List<LeaveHistoryResponse> data)
    {
        cache.Set(CacheKeys.LeaveHistory(employeeNo), data, CacheExpiration.LeaveHistory);
    }

    public static void InvalidateLeaveHistory(this ICacheService cache, string employeeNo)
    {
        cache.Remove(CacheKeys.LeaveHistory(employeeNo));
    }

    public static LeaveSummaryResponse? GetLeaveSummary(this ICacheService cache, string employeeNo)
    {
        return cache.Get<LeaveSummaryResponse>(CacheKeys.LeaveSummary(employeeNo));
    }

    public static void SetLeaveSummary(this ICacheService cache, string employeeNo, LeaveSummaryResponse data)
    {
        cache.Set(CacheKeys.LeaveSummary(employeeNo), data, CacheExpiration.LeaveSummary);
    }

    public static void InvalidateLeaveSummary(this ICacheService cache, string employeeNo)
    {
        cache.Remove(CacheKeys.LeaveSummary(employeeNo));
    }

    // Bulk invalidation
    public static void InvalidateAllUserCaches(this ICacheService cache, string employeeNo)
    {
        cache.InvalidateDashboard(employeeNo);
        cache.InvalidateLeaveHistory(employeeNo);
        cache.InvalidateLeaveSummary(employeeNo);
        
    }
}
