using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Application.Dtos.Dashboard;
using ESSPortal.Web.Blazor.Dtos.Leave;

namespace ESSPortal.Web.Blazor.Contracts.Interfaces.Common;

public interface IAppStateService
{
    Task<DashboardResponse> LoadDashboardDataAsync(string employeeNo, bool forceRefresh = false);
    Task<LeaveApplicationFormResponse> GetLeaveApplicationFormDataAsync(string employeeNo, bool forceRefresh = false);
    Task<CurrentUserResponse> LoadCurrentUserAsync(bool forceRefresh = false);


    void ClearCache(string? employeeNo = null);
    void ClearLeaveFormCache(string? employeeNo = null);


    // State Events
    event Action? OnStateChanged;
}
