using EssPortal.Domain.Enums.NavEnums;

using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Application.Utilities;
using ESSPortal.Domain.NavEntities;

namespace ESSPortal.Application.Mappings;
public static class BCEntityMappingExtensions
{
    public static List<LeaveApplicationCardResponse> ToLeaveApplicationCardResponse(this List<Domain.Entities.LeaveApplicationCard> entities)
    {
        if (entities == null) return [];

        return entities.Select(entity => new LeaveApplicationCardResponse
        {
            ApplicationNo = entity.Application_No ?? string.Empty,
            ApplicationDate = entity.Application_Date,
            ApplyOnBehalf = entity.Apply_on_behalf ?? false,
            EmployeeNo = entity.Employee_No,
            EmployeeName = entity.Employee_Name,
            EmailAddress = entity.Email_Adress,
            EmploymentType = entity.Employment_Type ?? "",
            ResponsibilityCenter = entity.Responsibility_Center,
            MobileNo = entity.Mobile_No ?? string.Empty,
            LeavePeriod = entity.Leave_Period ?? string.Empty,
            LeaveCode = entity.Leave_Code ?? string.Empty,
            LeaveStatus = entity.Leave_Status,
            Status = ParseLeaveApplicationCardStatus(entity.Status ?? string.Empty),
            LeaveEarnedToDate = entity.Leave_Earned_to_Date ?? 0,
            DaysApplied = entity.Days_Applied,
            StartDate = entity.Start_Date,
            EndDate = entity.End_Date,
            ResumptionDate = entity.Resumption_Date,
            DutiesTakenOverBy = entity.Duties_Taken_Over_By ?? string.Empty,
            RelievingName = entity.Relieving_Name ?? string.Empty,
            LeaveAllowancePayable = entity.Leave_Allowance_Payable ?? false
        }).ToList();
    }

    public static Domain.NavEntities.LeaveApplication.LeaveApplicationCard ToCreateLeaveApplicationCard(this CreateLeaveApplicationRequest request)
    {
        return new Domain.NavEntities.LeaveApplication.LeaveApplicationCard
        {
            employeeNumber = request.EmployeeNo,
            applyOnBehalf = request.ApplyOnBehalf,
            leaveCode = request.LeaveType,
            leaveStartDate = DateOnly.FromDateTime(request.FromDate),
            daysApplied = request.DaysApplied,
            dutiesTakenOverBy = request.DutiesTakenOverBy,

        };
    }

    public static Domain.NavEntities.LeaveApplication.LeaveApplicationCard ToUpdateLeaveApplicationCard(this CreateLeaveApplicationRequest request)
    {
        return new Domain.NavEntities.LeaveApplication.LeaveApplicationCard
        {
            applicationNo = request.ApplicationNo,
            leaveCode = request.LeaveType,
            leaveStartDate = DateOnly.FromDateTime(request.FromDate),
            daysApplied = request.DaysApplied,
            dutiesTakenOverBy = request.DutiesTakenOverBy,

        };
    }

    public static LeaveTypeResponse ToLeaveTypeResponse(this LeaveTypes leaveType)
    {
        return new LeaveTypeResponse
        {
            Code = leaveType.Code ?? string.Empty,
            Description = leaveType.Description ?? string.Empty,
            MaxDays = leaveType.Days ?? 0m,
            RequiresApproval = true, // Default business rule
            Days = leaveType.Days,
            Gender = leaveType.Gender?.ToString(),
            AnnualLeave = leaveType.Annual_Leave
        };
    }

    public static List<LeaveHistoryResponse> ToLeaveHistoryResponses(this List<Domain.Entities.LeaveApplicationCard> entities, List<LeaveTypeResponse>? leaveTypes = null)
    {
        if (entities == null || !entities.Any())
            return [];

        // Create lookup dictionary for performance

        return entities.Select(entity => new LeaveHistoryResponse
        {
            ApplicationNo = entity.Application_No ?? string.Empty,
            ApplicationDate = entity.Application_Date,
            LeaveType = entity.Leave_Code ?? "ANNUAL",
            StartDate = entity.Start_Date,
            EndDate = entity.End_Date,
            DaysApplied = entity.Days_Applied,
            DutiesTakenOverBy = entity.Duties_Taken_Over_By ?? string.Empty,
            Status = entity.Status ?? "Open",
            LeavePeriod = entity.Leave_Period ?? string.Empty,

            // Computed properties
            Duration = CalculateDuration(entity.Start_Date, entity.End_Date),
            DurationText = GetDurationText(entity.Start_Date, entity.End_Date, entity.Days_Applied),
            StatusDisplayText = GetLeaveApplicationListStatusDescription(ParseLeaveApplicationListStatus(entity.Status ?? "Open")),
            StatusCssClass = GetStatusCssClass(ParseLeaveApplicationListStatus(entity.Status)),
            StateText = GetLeaveApplicationListStatusDescription(ParseLeaveApplicationListStatus(entity.Status)),
            StateCssClass = GetStateCssClass(ParseLeaveApplicationListStatus(entity.Status)),
            IsCurrentYear = entity.Start_Date.Year == DateTime.Now.Year,
            IsApproved = IsApprovedStatus(ParseLeaveApplicationListStatus(entity.Status)),
            IsPending = IsPendingStatus(ParseLeaveApplicationListStatus(entity.Status)),
            IsRejected = IsRejectedStatus(ParseLeaveApplicationListStatus(entity.Status))
        })
        .OrderByDescending(h => h.ApplicationDate)
        .ToList();
    }

    public static List<ApprovedLeaveResponse> ToApprovedLeaveResponses(this List<ApprovedLeaves> entities)
    {
        if (entities == null || !entities.Any()) return [];

        return entities.Select(entity => new ApprovedLeaveResponse
        {
            ApplicationNo = entity.Application_No ?? string.Empty,
            ApplicationDate = entity.Application_Date ?? default,
            EmployeeNo = entity.Employee_No,
            EmployeeName = entity.Employee_Name,
            LeaveCode = entity.Leave_Code,
            DaysApplied = entity.Days_Applied,
            StartDate = entity.Start_Date,
            EndDate = entity.End_Date,
            Status = entity.Status ?? string.Empty,
            EmailAddress = entity.Email_Adress,

            // Computed properties
            Duration = CalculateDuration(entity.Start_Date, entity.End_Date),

            DurationText = GetDurationText(
                entity.Start_Date, 
                entity.End_Date, 
                entity.Days_Applied),

            IsCurrentYear = entity.Start_Date.Year == DateTime.Now.Year
        })
        .OrderByDescending(r => r.ApplicationDate)
        .ToList();
    }

    public static AnnualLeaveSummaryResponse ToAnnualLeaveSummaryResponse(
        string employeeNo,
        List<ApprovedLeaves>? approvedLeaves = null,
        List<Domain.Entities.LeaveApplicationCard>? leaveApplicationCards = null,
        List<LeaveTypes>? leaveTypes = null)
    {
        // Use LeaveApplicationCard method if data is available
        if (leaveApplicationCards?.Any() == true && leaveTypes?.Any() == true)
        {
            return LeaveStatisticsCalculator.CalculateAnnualLeaveStatistics(
                employeeNo,
                leaveApplicationCards,
                leaveTypes,
                approvedLeaves);
        }

        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;
        var leaveEarnedToDate = Math.Max(0, (currentMonth - 1) * 2.5m);

        return new AnnualLeaveSummaryResponse
        {
            TotalEntitlement = 30m,
            LeaveEarnedToDate = leaveEarnedToDate,
            TotalTaken = 0m,
            CurrentBalance = leaveEarnedToDate,
            UsagePercentage = 0m,
            AccumulatedDays = 0m,
            MonthlyEarned = 2.5m,
            MonthlyTaken = 0m,
            MonthlyBalance = 2.5m,
            PendingApplications = 0,
            ApprovedThisMonth = 0,
            CurrentPeriod = currentYear.ToString()
        };
    }

    public static AnnualLeaveSummaryResponse FromComprehensive(this LeaveSummaryResponse comprehensive)
    {
        var annual = comprehensive.AnnualLeave;
        return new AnnualLeaveSummaryResponse
        {
            TotalEntitlement = annual.YearlyEntitlement,
            LeaveEarnedToDate = annual.EarnedToDate,
            CurrentBalance = annual.CurrentBalance,
            TotalTaken = annual.TotalTaken,
            AccumulatedDays = annual.BroughtForward,
            UsagePercentage = annual.UsagePercentage,
            MonthlyEarned = annual.MonthlyEarned ?? 0,
            MonthlyTaken = annual.MonthlyTaken,
            MonthlyBalance = (annual.MonthlyEarned ?? 0) - annual.MonthlyTaken,
            PendingApplications = annual.PendingApplications,
            ApprovedThisMonth = annual.ApprovedThisMonth,
            CurrentPeriod = annual.CurrentPeriod
        };
    }

    public static LeaveSummaryResponse ToLeaveSummaryResponse(
        string employeeNo,
        List<LeaveTypes> leaveTypes,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards)
    {
        return NewLeaveStatisticsCalculator.CalculateLeaveTypeStatistics(
        employeeNo,
        leaveApplicationCards,
        leaveTypes ); 
       
    }




    #region Helper Methods


    private static int CalculateDuration(DateTime startDate, DateTime endDate)
    {
        return Math.Max(1, (int)(endDate.Date - startDate.Date).TotalDays + 1);
    }

    private static string GetDurationText(DateTime startDate, DateTime endDate, decimal daysApplied)
    {
        var calculatedDays = CalculateDuration(startDate, endDate);
        var displayDays = daysApplied > 0 ? (int)daysApplied : calculatedDays;

        if (startDate.Date == endDate.Date)
        {
            return $"{startDate:MMM dd, yyyy} ({displayDays} day{(displayDays != 1 ? "s" : "")})";
        }

        return $"{startDate:MMM dd} - {endDate:MMM dd, yyyy} ({displayDays} day{(displayDays != 1 ? "s" : "")})";
    }

    private static string GetLeaveApplicationListStatusDescription(LeaveApplicationListStatus status)
    {
        return status switch
        {
            LeaveApplicationListStatus.Released => "Approved",
            LeaveApplicationListStatus.Open => "Draft",
            LeaveApplicationListStatus.Pending_Approval => "Pending Approval",
            LeaveApplicationListStatus.Pending_Prepayment => "Pending Payment",
            LeaveApplicationListStatus.Rejected => "Rejected",
            _ => "Under Review"
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

    private static bool IsApprovedStatus(LeaveApplicationListStatus status)
    {
        return status == LeaveApplicationListStatus.Released;
    }

    private static bool IsPendingStatus(LeaveApplicationListStatus status)
    {
        return status is LeaveApplicationListStatus.Open or
                        LeaveApplicationListStatus.Pending_Approval or
                        LeaveApplicationListStatus.Pending_Prepayment;
    }

    private static bool IsRejectedStatus(LeaveApplicationListStatus status)
    {
        return status == LeaveApplicationListStatus.Rejected;
    }

    #endregion

    #region Status Parsing Methods

    private static LeaveApplicationListStatus ParseLeaveApplicationListStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return LeaveApplicationListStatus.Open;

        return status.ToUpperInvariant() switch
        {
            "RELEASED" => LeaveApplicationListStatus.Released,
            "PENDING APPROVAL" => LeaveApplicationListStatus.Pending_Approval,
            "PENDING_APPROVAL" => LeaveApplicationListStatus.Pending_Approval,
            "PENDING_PREPAYMENT" => LeaveApplicationListStatus.Pending_Prepayment,
            "PENDING PREPAYMENT" => LeaveApplicationListStatus.Pending_Prepayment,
            "REJECTED" => LeaveApplicationListStatus.Rejected,
            "OPEN" => LeaveApplicationListStatus.Open,
            _ => LeaveApplicationListStatus.Open
        };
    }

    private static LeaveApplicationCardStatus ParseLeaveApplicationCardStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return LeaveApplicationCardStatus.Open;

        return status.ToUpperInvariant() switch
        {
            "RELEASED" => LeaveApplicationCardStatus.Released,
            "PENDING APPROVAL" => LeaveApplicationCardStatus.Pending_Approval,
            "PENDING_APPROVAL" => LeaveApplicationCardStatus.Pending_Approval,
            "PENDING_PREPAYMENT" => LeaveApplicationCardStatus.Pending_Prepayment,
            "PENDING PREPAYMENT" => LeaveApplicationCardStatus.Pending_Prepayment,
            "REJECTED" => LeaveApplicationCardStatus.Rejected,
            "OPEN" => LeaveApplicationCardStatus.Open,
            _ => LeaveApplicationCardStatus.Open
        };
    }

    

    #endregion
}

