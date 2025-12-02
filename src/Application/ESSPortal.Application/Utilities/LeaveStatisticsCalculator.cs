using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.NavEntities;

using Microsoft.Extensions.Logging;

namespace ESSPortal.Application.Utilities;

public static class LeaveStatisticsCalculator
{
    // Add this for better debugging
    private static ILogger? _logger;

    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculate leave statistics using LeaveApplicationCard as primary source
    /// </summary>
    public static AnnualLeaveSummaryResponse CalculateAnnualLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes,
        List<ApprovedLeaves>? approvedLeaves = null,
        List<LeaveApplicationList>? leaveApplications = null)
    {
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;

        // 1. Get annual entitlement (default 30, but could be from LeaveTypes)
        var annualLeaveType = leaveTypes?.FirstOrDefault(lt => lt.Annual_Leave == true);
        var annualEntitlement = annualLeaveType?.Days ?? 30m;

        // 2. Get actual Leave Earned to Date from system (not calculated)
        var leaveEarnedToDate = GetLeaveEarnedToDate(employeeNo, currentYear, leaveApplicationCards);
        _logger?.LogInformation("Leave Earned to Date (from system): {Earned}", leaveEarnedToDate);

        // 3. Calculate total days taken this year
        var daysTakenCurrentYear = CalculateDaysTakenCurrentYear(employeeNo, currentYear, leaveApplicationCards);
        _logger?.LogInformation("Days Taken Current Year: {Taken}", daysTakenCurrentYear);

        // 4. Calculate brought forward from previous year
        var broughtForward = CalculateBroughtForward(employeeNo, currentYear - 1, leaveApplicationCards, leaveTypes);
        _logger?.LogInformation("Brought Forward: {BroughtForward}", broughtForward);

        // 5. Calculate current balance = entitlement + brought forward - taken
        var currentBalance = annualEntitlement + broughtForward - daysTakenCurrentYear;
        _logger?.LogInformation("Current Balance: {Entitlement} + {BroughtForward} - {Taken} = {Balance}",
            annualEntitlement, broughtForward, daysTakenCurrentYear, currentBalance);

        // 6. Usage percentage based on total available
        var totalAvailable = annualEntitlement + broughtForward;
        var usagePercentage = totalAvailable > 0
            ? Math.Round((daysTakenCurrentYear / totalAvailable) * 100, 1)
            : 0m;

        // 7. Other calculations
        var pendingApplications = CalculatePendingApplications(employeeNo, currentYear, leaveApplicationCards);
        var monthlyTaken = CalculateMonthlyTaken(employeeNo, currentYear, currentMonth, leaveApplicationCards);
        var approvedThisMonth = CalculateApprovedThisMonth(employeeNo, currentYear, currentMonth, leaveApplicationCards);

        var result = new AnnualLeaveSummaryResponse
        {
            TotalEntitlement = annualEntitlement,
            LeaveEarnedToDate = leaveEarnedToDate,
            TotalTaken = daysTakenCurrentYear,
            CurrentBalance = Math.Max(0, currentBalance), // Ensure non-negative
            UsagePercentage = usagePercentage,
            AccumulatedDays = broughtForward,
            MonthlyEarned = 2.5m, // Standard monthly accrual
            MonthlyTaken = monthlyTaken,
            MonthlyBalance = 2.5m - monthlyTaken,
            PendingApplications = pendingApplications,
            ApprovedThisMonth = approvedThisMonth,
            CurrentPeriod = currentYear.ToString()
        };

        _logger?.LogInformation("=== FINAL RESULT: Earned={Earned}, Taken={Taken}, Balance={Balance}, Usage={Usage}% ===",
            result.LeaveEarnedToDate, result.TotalTaken, result.CurrentBalance, result.UsagePercentage);

        return result;
    }

    private static decimal GetLeaveEarnedToDate(string employeeNo, int currentYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards)
    {
        if (leaveApplicationCards?.Any() != true)
        {
            // Fallback to calculation if no data
            var currentMonth = DateTime.Now.Month;
            return Math.Max(0, (currentMonth - 1) * 2.5m);
        }

        // Get the most recent Leave_Earned_to_Date for this employee and year
        var earnedToDate = leaveApplicationCards
            .Where(card => !string.IsNullOrWhiteSpace(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString())
            .OrderByDescending(card => card.Application_Date)
            .FirstOrDefault()?.Leave_Earned_to_Date ?? 0m;

        if (earnedToDate == 0m)
        {
            var currentMonth = DateTime.Now.Month;
            earnedToDate = Math.Max(0, (currentMonth - 1) * 2.5m);
        }


        _logger?.LogInformation("Leave Earned to Date from LeaveApplicationCard: {Earned}", earnedToDate);
        return earnedToDate;
    }

    private static decimal CalculateDaysTakenCurrentYear(string employeeNo, int currentYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards)
    {
        if (leaveApplicationCards?.Any() != true) return 0m;

        var daysTaken = leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          card.Status == "Released" &&
                          card.Leave_Code == "ANNUAL") // Focus on annual leave
            .Sum(card => card.Days_Applied);

        _logger?.LogInformation("Days taken current year from LeaveApplicationCard: {Days}", daysTaken);
        return daysTaken;
    }

    private static decimal CalculateBroughtForward(string employeeNo, int previousYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards, List<Domain.NavEntities.LeaveTypes>? leaveTypes)
    {
        if (leaveApplicationCards?.Any() != true) return 0m;

        // Calculate previous year usage
        var previousYearTaken = leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == previousYear.ToString() &&
                          card.Status == "Released" &&
                          card.Leave_Code == "ANNUAL")
            .Sum(card => card.Days_Applied);

        // Calculate unutilized leave
        var unutilized = Math.Max(0, 30m - previousYearTaken);

        // Apply the 10-day carry-forward limit as per your rule
        var broughtForward = Math.Min(unutilized, 10m);

        _logger?.LogInformation("Previous year ({Year}) taken: {Taken}, Unutilized: {Unutilized}, Brought forward: {BroughtForward} (max 10 days)", previousYear, previousYearTaken, unutilized, broughtForward);
             
        return broughtForward;
    }

    private static int CalculatePendingApplications(string employeeNo, int currentYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards)
    {
        if (leaveApplicationCards?.Any() != true) return 0;

        string[] pendingStatuses = ["Open", "Pending Approval", "Being Processed"];

        return leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          pendingStatuses.Contains(card.Leave_Status, StringComparer.OrdinalIgnoreCase))
            .Count();
    }

    private static decimal CalculateMonthlyTaken(string employeeNo, int currentYear, int currentMonth, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards)
    {
        if (leaveApplicationCards?.Any() != true) return 0m;

        return leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          card.Start_Date.Year == currentYear &&
                          card.Start_Date.Month == currentMonth &&
                          card.Status == "Released" &&
                          card.Leave_Code == "ANNUAL")

            .Sum(card => card.Days_Applied);
    }

    private static int CalculateApprovedThisMonth(string employeeNo, int currentYear, int currentMonth, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards)
    {
        if (leaveApplicationCards?.Any() != true) return 0;

        return leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          card.Start_Date.Year == currentYear &&
                          card.Start_Date.Month == currentMonth &&
                          card.Status == "Released")

            .Count();
    }


   







}
