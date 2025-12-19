using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Web.Blazor.Dtos.Leave;
using ESSPortal.Web.Blazor.ViewModels.Leave;

namespace ESSPortal.Web.Blazor.ViewModels.Dashboard;

public class DashboardViewModel
{
    public string? EmployeeNo { get; set; }
    public string? EmployeeName { get; set; }

    // Backend DTOs used directly
    public LeaveSummaryResponse? LeaveSummary { get; set; } // Comprehensive (all leave types)
    public AnnualLeaveSummaryResponse? AnnualLeaveSummary { get; set; } // Annual leave only

    // Frontend ViewModels for complex display logic
    public LeaveSummaryViewModel? LeaveSummaryViewModel { get; set; } // For dashboard table

    // Collections
    public List<LeaveHistoryResponse> LeaveHistory { get; set; } = [];
    public List<LeaveApplicationCardResponse> LeaveApplicationCards { get; set; } = [];
    public List<LeaveTypeResponse> LeaveTypes { get; set; } = [];

    // Dashboard-specific state properties (use backend DTOs)
    public bool HasData => LeaveSummary != null || AnnualLeaveSummary != null;
    public bool HasLeaveHistory => LeaveHistory.Any() == true;
    public bool ShouldShowEmptyState => !HasData || (AnnualLeaveSummary?.CurrentBalance ?? 0) == 0;
    public bool HasBroughtForward => (AnnualLeaveSummary?.AccumulatedDays ?? 0) > 0;
    public bool HasMonthlyActivity => (AnnualLeaveSummary?.MonthlyTaken ?? 0) > 0 || (AnnualLeaveSummary?.ApprovedThisMonth ?? 0) > 0;

    // Display helpers (simple formatting, no business logic)
    public string WelcomeMessage => $"Welcome back, {EmployeeName}!";
    public int TotalApplicationsCount => LeaveHistory?.Count ?? 0;

    // Quick access properties (delegate to backend DTOs)
    public decimal CurrentBalance => AnnualLeaveSummary?.CurrentBalance ?? 0;
    public decimal UsagePercentage => AnnualLeaveSummary?.UsagePercentage ?? 0;
    public bool IsLowBalance => AnnualLeaveSummary?.IsLowBalance ?? false;
    public bool IsHighUsage => AnnualLeaveSummary?.IsHighUsage ?? false;

    // Quick access to individual leave type balances (for dashboard cards)
    public decimal AnnualLeaveBalance => LeaveSummary?.AnnualLeave.CurrentBalance ?? 0;
    public decimal CompassionLeaveBalance => LeaveSummary?.CompassionLeave?.CurrentBalance ?? 0;
    public decimal SickLeaveBalance => LeaveSummary?.SickLeave?.CurrentBalance ?? 0;
    public decimal MaternityLeaveBalance => LeaveSummary?.MaternityLeave.CurrentBalance ?? 0;
    public decimal PaternityLeaveBalance => LeaveSummary?.PaternityLeave?.CurrentBalance ?? 0;
    public decimal StudyLeaveBalance => LeaveSummary?.StudyLeave?.CurrentBalance ?? 0;
    public decimal UnpaidLeaveBalance => LeaveSummary?.UnpaidLeave?.CurrentBalance ?? 0;
}