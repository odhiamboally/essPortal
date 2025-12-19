using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Dtos.Dashboard;
using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Application.Utilities;

namespace ESSPortal.Application.Extensions;
public static class CacheServiceExtensions
{

    // USerInfo methods
    public static UserInfo? GetUserInfo(this ICacheService cache, string employeeNo)
    {
        return cache.Get<UserInfo>(CacheKeys.UserInfo(employeeNo));
    }

    public static void SetUserInfo(this ICacheService cache, string employeeNo, UserInfo data)
    {
        cache.Set(CacheKeys.UserInfo(employeeNo), data, CacheExpiration.Dashboard);
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

    public static AnnualLeaveSummaryResponse? GetAnnualLeaveSummary(this ICacheService cache, string employeeNo)
    {
        return cache.Get<AnnualLeaveSummaryResponse>(CacheKeys.AnnualLeaveSummary(employeeNo));
    }

    public static void SetAnnualLeaveSummary(this ICacheService cache, string employeeNo, AnnualLeaveSummaryResponse data)
    {
        cache.Set(CacheKeys.AnnualLeaveSummary(employeeNo), data, CacheExpiration.AnnualLeaveSummary);
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
        // Note: We don't invalidate leave types as they're shared across users
    }
}
