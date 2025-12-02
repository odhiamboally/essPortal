using EssPortal.Domain.Enums.NavEnums;

using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.NavEntities;
using LeaveApplicationCard = ESSPortal.Domain.Entities.LeaveApplicationCard;

namespace ESSPortal.Application.Utilities;

public static class LeaveSummaryCalculator
{
    /// <summary>
    /// Calculates leave summary from BC data using actual BC leave earning logic
    /// BC Logic: 30 days/year = 2.5 days/month accumulated over time
    /// </summary>
    public static AnnualLeaveSummaryResponse CalculateLeaveSummary(List<LeaveApplicationCard> leaveApplicationCards, List<LeaveApplicationList> leaveApplicationLists, string? currentLeavePeriod = null)
    {
        currentLeavePeriod ??= DateTime.Now.Year.ToString();

        // Filter for current period
        var currentPeriodCards = leaveApplicationCards.Where(card => card.Leave_Period == currentLeavePeriod).ToList();
            
        var currentPeriodLists = leaveApplicationLists.Where(list => list.Leave_Period == currentLeavePeriod).ToList();
            
        // Get the most recent Leave_Earned_to_Date from LeaveApplicationCard
        // This represents the accumulated leave days earned up to the latest application
        var latestCard = currentPeriodCards.OrderByDescending(card => card.Application_Date).FirstOrDefault();
            
        decimal leaveEarnedToDate = latestCard?.Leave_Earned_to_Date ?? CalculateEarnedToDate();

        // Calculate total days taken (approved/released applications only)
        string[] approvedStatuses = ["Released"];
        var totalTakenThisYear = currentPeriodLists.Where(app => approvedStatuses.Contains(app.Status)).Sum(app => app.Days_Applied ?? 0);
            
        // Calculate pending applications

        // Calculate pending applications
        var pendingApplicationsCount = currentPeriodLists.Where(app => IsPendingStatusString(app.Status)).ToList().Count;
            
        // Current balance = Earned to date - Total taken
        var currentBalance = Math.Max(0, leaveEarnedToDate - totalTakenThisYear);

        // Monthly calculations
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        // Monthly earned is fixed at 2.5 days (30 days / 12 months)
        var monthlyEarned = 2.5m;

        // Calculate days taken this month
        var currentMonthApplications = currentPeriodLists.Where(app => app.Start_Date.Month == currentMonth && app.Start_Date.Year == currentYear).ToList();

        var monthlyTaken = currentMonthApplications.Where(app => approvedStatuses.Contains(app.Status)).Sum(app => app.Days_Applied ?? 0);

        var approvedThisMonth = currentMonthApplications.Where(app => approvedStatuses.Contains(app.Status)).Count();
            
            
        // Calculate usage percentage
        var annualEntitlement = 30m;
        var usagePercentage = leaveEarnedToDate > 0 ? Math.Round(totalTakenThisYear / leaveEarnedToDate * 100, 1) : 0;

        return new AnnualLeaveSummaryResponse
        {
            TotalEntitlement = annualEntitlement,
            CurrentBalance = Math.Round(currentBalance, 1),
            TotalTaken = totalTakenThisYear,
            MonthlyEarned = monthlyEarned,
            MonthlyTaken = monthlyTaken,
            MonthlyBalance = Math.Round(monthlyEarned - monthlyTaken, 1),
            PendingApplications = pendingApplicationsCount,
            ApprovedThisMonth = approvedThisMonth,
            UsagePercentage = usagePercentage,
            CurrentPeriod = currentLeavePeriod,

            // BC-specific properties
            LeaveEarnedToDate = leaveEarnedToDate,
            AccumulatedDays = Math.Max(0, leaveEarnedToDate - totalTakenThisYear)
        };
    }

    /// <summary>
    /// Calculates earned leave to date based on current month
    /// BC Logic: 2.5 days earned per month
    /// </summary>
    private static decimal CalculateEarnedToDate()
    {
        var currentMonth = DateTime.Now.Month;
        return currentMonth * 2.5m; // 2.5 days per month
    }

    /// <summary>
    /// Converts BC enum status to display string
    /// </summary>
    public static string GetDisplayStatus(LeaveApplicationCardStatus status)
    {
        return status switch
        {
            LeaveApplicationCardStatus.Released => "Approved",
            LeaveApplicationCardStatus.Open => "Draft",
            LeaveApplicationCardStatus.Pending_Approval => "Pending Approval",
            LeaveApplicationCardStatus.Pending_Prepayment => "Pending Payment",
            LeaveApplicationCardStatus.Rejected => "Rejected",
            _ => status.ToString().Replace("_", " ")
        };
    }

    /// <summary>
    /// Converts BC enum Leave_Status to display string
    /// </summary>
    public static string GetDisplayStatus(Leave_Status status)
    {
        return status switch
        {
            Leave_Status.Being_Processed => "Under Review",
            Leave_Status.Approved => "Approved",
            Leave_Status.Rejected => "Rejected",
            Leave_Status.Canceled => "Cancelled",
            _ => status.ToString().Replace("_", " ")
        };
    }

    /// <summary>
    /// Converts BC enum LeaveApplicationListStatus to display string
    /// </summary>
    public static string GetDisplayStatus(LeaveApplicationListStatus status)
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

    /// <summary>
    /// Determines if the application is approved based on BC enum status
    /// </summary>
    public static bool IsApprovedStatus(string bcStatus)
    {
        string[] approvedStatuses = ["Released", "Approved"];
        return approvedStatuses.Contains(bcStatus, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsPendingStatus(string bcStatus)
    {
        string[] pendingStatuses = ["Open", "Pending Approval", "Pending_Approval", "Pending_Prepayment", "Being Processed"
        ];
        return pendingStatuses.Contains(bcStatus, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsRejectedStatus(string bcStatus)
    {
        string[] rejectedStatuses = ["Rejected", "Cancelled", "Canceled"];
        return rejectedStatuses.Contains(bcStatus, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsApprovedStatus(LeaveApplicationCardStatus status)
    {
        return status == LeaveApplicationCardStatus.Released;
    }

    public static bool IsApprovedStatus(Leave_Status status)
    {
        return status == Leave_Status.Approved;
    }

    /// <summary>
    /// Determines if the application is pending based on BC enum status
    /// </summary>
    public static bool IsPendingStatus(LeaveApplicationListStatus status)
    {
        return status == LeaveApplicationListStatus.Open ||
               status == LeaveApplicationListStatus.Pending_Approval ||
               status == LeaveApplicationListStatus.Pending_Prepayment;
    }

    public static bool IsPendingStatus(LeaveApplicationCardStatus status)
    {
        return status == LeaveApplicationCardStatus.Open ||
               status == LeaveApplicationCardStatus.Pending_Approval ||
               status == LeaveApplicationCardStatus.Pending_Prepayment;
    }

    public static bool IsPendingStatus(Leave_Status status)
    {
        return status == Leave_Status.Being_Processed;
    }

    /// <summary>
    /// Determines if the application is rejected
    /// </summary>
    public static bool IsRejectedStatus(LeaveApplicationListStatus status)
    {
        return status == LeaveApplicationListStatus.Rejected;
    }

    public static bool IsRejectedStatus(LeaveApplicationCardStatus status)
    {
        return status == LeaveApplicationCardStatus.Rejected;
    }

    public static bool IsRejectedStatus(Leave_Status status)
    {
        return status == Leave_Status.Rejected || status == Leave_Status.Canceled;
    }

    public static bool IsPendingStatusString(string status)
    {
        string[] pendingStatusStrings = ["Open", "Pending Approval", "Pending_Approval", "Pending_Prepayment"];
        return pendingStatusStrings.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
    }
}
