using ESSPortal.Shared.Contracts.Interfaces.Common;
using ESSPortal.Shared.Dtos.Dashboard;
using ESSPortal.Shared.Dtos.Leave;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;

using ESSPortal.Web.Mvc.Utilities.Session;

namespace ESSPortal.Web.Mvc.Extensions;

public static class CacheServiceExtensions
{
    // Session methods
    extension(ICacheService cache)
    {
        public string? GetSessionId(string employeeNo)
        {
            return cache.Get<string>(CacheKeys.SessionId(employeeNo));
        }

        public void SetSessionId(string employeeNo, string data)
        {
            cache.Set(CacheKeys.SessionId(employeeNo), data, CacheExpiration.SessionId);
        }

        public UserInfo? GetUserInfo(string employeeNo)
        {
            return cache.Get<UserInfo>(CacheKeys.UserInfo(employeeNo));
        }

        public void SetUserInfo(string employeeNo, UserInfo data)
        {
            cache.Set(CacheKeys.UserInfo(employeeNo), data, CacheExpiration.UserInfo);
        }

        public DashboardResponse? GetDashboard(string employeeNo)
        {
            return cache.Get<DashboardResponse>(CacheKeys.Dashboard(employeeNo));
        }

        public void SetDashboard(string employeeNo, DashboardResponse data)
        {
            cache.Set(CacheKeys.Dashboard(employeeNo), data, CacheExpiration.Dashboard);
        }

        public void InvalidateDashboard(string employeeNo)
        {
            cache.Remove(CacheKeys.Dashboard(employeeNo));
        }

        public List<LeaveTypeResponse>? GetLeaveTypes()
        {
            return cache.Get<List<LeaveTypeResponse>>(CacheKeys.LeaveTypes());
        }

        public void SetLeaveTypes(List<LeaveTypeResponse> data)
        {
            cache.Set(CacheKeys.LeaveTypes(), data, CacheExpiration.LeaveTypes);
        }

        public void InvalidateLeaveTypes()
        {
            cache.Remove(CacheKeys.LeaveTypes());
        }

        public List<LeaveHistoryResponse>? GetLeaveHistory(string employeeNo)
        {
            return cache.Get<List<LeaveHistoryResponse>>(CacheKeys.LeaveHistory(employeeNo));
        }

        public void SetLeaveHistory(string employeeNo, List<LeaveHistoryResponse> data)
        {
            cache.Set(CacheKeys.LeaveHistory(employeeNo), data, CacheExpiration.LeaveHistory);
        }

        public void InvalidateLeaveHistory(string employeeNo)
        {
            cache.Remove(CacheKeys.LeaveHistory(employeeNo));
        }

        public LeaveSummaryResponse? GetLeaveSummary(string employeeNo)
        {
            return cache.Get<LeaveSummaryResponse>(CacheKeys.LeaveSummary(employeeNo));
        }

        public void SetLeaveSummary(string employeeNo, LeaveSummaryResponse data)
        {
            cache.Set(CacheKeys.LeaveSummary(employeeNo), data, CacheExpiration.LeaveSummary);
        }

        public void InvalidateLeaveSummary(string employeeNo)
        {
            cache.Remove(CacheKeys.LeaveSummary(employeeNo));
        }

        public void InvalidateAllUserCaches(string employeeNo)
        {
            cache.InvalidateDashboard(employeeNo);
            cache.InvalidateLeaveHistory(employeeNo);
            cache.InvalidateLeaveSummary(employeeNo);
        
        }
    }

    // UserInfo methods

    // Dashboard methods

    // Leave methods

    // Bulk invalidation
}
