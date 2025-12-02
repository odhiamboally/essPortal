using EssPortal.Web.Mvc.Enums.NavEnums;
using EssPortal.Web.Mvc.Models.Navision;

using ESSPortal.Web.Mvc.Dtos.Dashboard;
using ESSPortal.Web.Mvc.Dtos.Leave;
using ESSPortal.Web.Mvc.ViewModels.Dashboard;
using ESSPortal.Web.Mvc.ViewModels.Leave;

namespace ESSPortal.Web.Mvc.Mappings;

public static class DashboardMappingExtensions
{
    public static DashboardViewModel ToDashboardViewModel(this DashboardResponse response)
    {
        return new DashboardViewModel
        {
            EmployeeNo = response.EmployeeNo,
            EmployeeName = response.EmployeeName,
            LeaveSummary = response.LeaveSummary, 
            AnnualLeaveSummary = response.AnnualLeaveSummary, 
            LeaveSummaryViewModel = BuildLeaveSummaryViewModel(response.LeaveSummary, response.AnnualLeaveSummary),
            LeaveHistory = response.LeaveHistory ?? [],
            LeaveApplicationCards = response.LeaveApplicationCards ?? [],
            LeaveTypes = response.LeaveTypes ?? []
        };
    }

    public static LeaveSummaryViewModel BuildLeaveSummaryViewModel(LeaveSummaryResponse? comprehensive, AnnualLeaveSummaryResponse? annual)
    {
        var viewModel = new LeaveSummaryViewModel();

        if (comprehensive == null)
        {
            // Fallback: create basic view with annual leave only
            if (annual != null)
            {
                viewModel.LeaveTypeRows.Add(CreateLeaveTypeRowFromAnnual(annual));
            }

            return viewModel;
        }

        // Create rows for each leave type from comprehensive data
        viewModel.LeaveTypeRows =
        [
            CreateLeaveTypeRow(comprehensive.AnnualLeave),
            CreateLeaveTypeRow(comprehensive.AdoptionLeave),
            CreateLeaveTypeRow(comprehensive.CompassionLeave),
            CreateLeaveTypeRow(comprehensive.MaternityLeave),
            CreateLeaveTypeRow(comprehensive.PaternityLeave),
            CreateLeaveTypeRow(comprehensive.SickLeave),
            CreateLeaveTypeRow(comprehensive.StudyLeave),
            CreateLeaveTypeRow(comprehensive.UnpaidLeave)
        ];

        return viewModel;
    }

    private static LeaveTypeRowViewModel CreateLeaveTypeRow(LeaveTypeStatistics leaveType)
    {
        return new LeaveTypeRowViewModel
        {
            LeaveType = leaveType.LeaveDescription,
            LeaveCode = leaveType.LeaveCode,
            AnnualEntitlement = leaveType.YearlyEntitlement,
            BroughtForward = leaveType.BroughtForward,
            Taken = leaveType.TotalTaken,
            TotalBalance = leaveType.CurrentBalance
        };
    }

    private static LeaveTypeRowViewModel CreateLeaveTypeRowFromAnnual(AnnualLeaveSummaryResponse annual)
    {
        return new LeaveTypeRowViewModel
        {
            LeaveType = "Annual Leave",
            LeaveCode = "ANNUAL",
            AnnualEntitlement = annual.TotalEntitlement,
            BroughtForward = annual.AccumulatedDays,
            Taken = annual.TotalTaken,
            TotalBalance = annual.CurrentBalance
        };
    }

    public static LeaveHistoryResponse ToLeaveHistoryResponse(this LeaveApplicationList entity, Dictionary<string, string> leaveTypeLookup)
    {
        var duration = (int)(entity.End_Date - entity.Start_Date).TotalDays + 1;
        var leaveTypeName = leaveTypeLookup.TryGetValue(entity.LeaveTypeId.ToString(), out var typeName)
            ? typeName
            : entity.LeaveTypeName ?? "Annual Leave";

        return new LeaveHistoryResponse
        {
            ApplicationNo = entity.Application_No ?? string.Empty,
            ApplicationDate = entity.Application_Date,
            LeaveType = leaveTypeName,
            StartDate = entity.Start_Date,
            EndDate = entity.End_Date,
            DaysApplied = entity.Days_Applied,
            Status = entity.Status,
            LeavePeriod = entity.LeavePeriod ?? string.Empty,

            Duration = duration,
            DurationText = GetDurationText(entity.Start_Date, entity.End_Date, entity.Days_Applied),
            StatusDisplayText = GetDisplayStatus(entity.Status),
            StatusCssClass = GetStatusCssClass(entity.Status),
            StateText = GetStateText(entity.Status),
            StateCssClass = GetStateCssClass(entity.Status),
            IsCurrentYear = entity.Start_Date.Year == DateTime.Now.Year,
            IsApproved = entity.Status == LeaveApplicationListStatus.Released,
            IsPending = IsStatusPending(entity.Status),
            IsRejected = entity.Status == LeaveApplicationListStatus.Rejected
        };
    }

    private static string GetDurationText(DateTime startDate, DateTime endDate, decimal daysApplied)
    {
        var calculatedDays = (int)(endDate - startDate).TotalDays + 1;
        var displayDays = (int)daysApplied > 0 ? (int)daysApplied : calculatedDays;

        if (startDate.Date == endDate.Date)
        {
            return $"{startDate:MMM dd, yyyy} ({displayDays} day)";
        }

        return $"{startDate:MMM dd} - {endDate:MMM dd, yyyy} ({displayDays} days)";
    }

    private static string GetDisplayStatus(LeaveApplicationListStatus status)
    {
        return status switch
        {
            LeaveApplicationListStatus.Released => "Approved",
            LeaveApplicationListStatus.Open => "Draft",
            LeaveApplicationListStatus.Pending_Approval => "Pending Approval",
            LeaveApplicationListStatus.Pending_Prepayment => "Pending Payment",
            LeaveApplicationListStatus.Rejected => "Rejected",
            _ => status.ToString().Replace("_", " ")
        };
    }

    private static string GetStatusCssClass(LeaveApplicationListStatus status)
    {
        return status switch
        {
            LeaveApplicationListStatus.Released => "status-approved",
            LeaveApplicationListStatus.Open => "status-draft",
            LeaveApplicationListStatus.Pending_Approval => "status-pending",
            LeaveApplicationListStatus.Pending_Prepayment => "status-pending",
            LeaveApplicationListStatus.Rejected => "status-rejected",
            _ => "status-pending"
        };
    }

    private static string GetStateText(LeaveApplicationListStatus status)
    {
        return status switch
        {
            LeaveApplicationListStatus.Released => "Completed",
            LeaveApplicationListStatus.Open => "Draft",
            LeaveApplicationListStatus.Pending_Approval => "Under Review",
            LeaveApplicationListStatus.Pending_Prepayment => "Under Review",
            LeaveApplicationListStatus.Rejected => "Closed",
            _ => "Under Review"
        };
    }

    private static string GetStateCssClass(LeaveApplicationListStatus status)
    {
        return status switch
        {
            LeaveApplicationListStatus.Released => "state-completed",
            LeaveApplicationListStatus.Open => "state-draft",
            LeaveApplicationListStatus.Pending_Approval => "state-review",
            LeaveApplicationListStatus.Pending_Prepayment => "state-review",
            LeaveApplicationListStatus.Rejected => "state-closed",
            _ => "state-review"
        };
    }

    private static bool IsStatusPending(LeaveApplicationListStatus status)
    {
        return status is LeaveApplicationListStatus.Open or
                        LeaveApplicationListStatus.Pending_Approval or
                        LeaveApplicationListStatus.Pending_Prepayment;
    }



}
