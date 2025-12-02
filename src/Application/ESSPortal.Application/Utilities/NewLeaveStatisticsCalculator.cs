using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Domain.NavEntities;

using Microsoft.Extensions.Logging;

namespace ESSPortal.Application.Utilities;
public static class NewLeaveStatisticsCalculator
{
    private static ILogger? _logger;

    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculate comprehensive leave statistics for all leave types
    /// </summary>
    public static LeaveSummaryResponse CalculateLeaveTypeStatistics(
    string employeeNo,
    List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
    List<LeaveTypes> leaveTypes,
    List<ApprovedLeaves>? approvedLeaves = null)
    {
        var currentYear = DateTime.Now.Year;

        _logger?.LogInformation("=== CALCULATING COMPREHENSIVE LEAVE STATISTICS FOR {EmployeeNo} ===", employeeNo);

        // Use object initializer to set init-only properties
        var annualLeaveTypeStats = CalculateAnnualLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes);
        var adoptionLeaveTypeStats = CalculateAdoptionLeaveStatistics(employeeNo, leaveApplicationCards, leaveTypes);
        var compassionLeaveTypeStats = CalculateCompassionLeaveStatistics(employeeNo, leaveApplicationCards, leaveTypes);
        var maternityLeaveTypeStats = CalculateMaternityLeaveStatistics(employeeNo, leaveApplicationCards, leaveTypes);
        var paternityLeaveTypeStats = CalculatePaternityLeaveStatistics(employeeNo, leaveApplicationCards, leaveTypes);
        var sickLeaveTypeStats = CalculateSickLeaveStatistics(employeeNo, leaveApplicationCards, leaveTypes);
        var studyLeaveTypeStats = CalculateStudyLeaveStatistics(employeeNo, leaveApplicationCards, leaveTypes);
        var unpaidLeaveTypeStats = CalculateUnpaidLeaveStatistics(employeeNo, leaveApplicationCards, leaveTypes);

        var tempResult = new LeaveSummaryResponse
        {
            EmployeeNo = employeeNo,
            CurrentPeriod = currentYear.ToString(),
            AnnualLeave = annualLeaveTypeStats,
            AdoptionLeave = adoptionLeaveTypeStats, 
            CompassionLeave = compassionLeaveTypeStats,
            MaternityLeave = maternityLeaveTypeStats,
            PaternityLeave = paternityLeaveTypeStats,
            SickLeave = sickLeaveTypeStats,
            StudyLeave = studyLeaveTypeStats,
            UnpaidLeave = unpaidLeaveTypeStats,
            TotalPendingApplications = 0, // will set below
            TotalApprovedThisMonth = 0,   // will set below
            TotalMonthlyTaken = 0         // will set below
        };

        // Calculate overall summary
        tempResult = tempResult with
        {
            TotalPendingApplications = tempResult.GetAllLeaveTypes().Sum(lt => lt.PendingApplications),
            TotalApprovedThisMonth = tempResult.GetAllLeaveTypes().Sum(lt => lt.ApprovedThisMonth),
            TotalMonthlyTaken = tempResult.GetAllLeaveTypes().Sum(lt => lt.MonthlyTaken)
        };

        _logger?.LogInformation("=== COMPREHENSIVE STATISTICS COMPLETED FOR {EmployeeNo} ===", employeeNo);
        return tempResult;
    }

    /// <summary>
    /// Calculate annual leave statistics (2.5 monthly accrual, carry forward allowed)
    /// </summary>
    public static LeaveTypeStatistics CalculateAnnualLeaveTypeStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes)
    {
        const string leaveCode = "ANNUAL";
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;

        _logger?.LogInformation("Calculating Annual Leave Statistics for {EmployeeNo}", employeeNo);

        var annualLeaveType = leaveTypes?.FirstOrDefault(lt => lt.Annual_Leave == true);
        var yearlyEntitlement = annualLeaveType?.Days ?? 30m;

        var earnedToDate = GetLeaveEarnedToDate(employeeNo, currentYear, leaveApplicationCards, leaveCode);
        var totalTaken = CalculateTotalTakenForLeaveType(employeeNo, currentYear, leaveApplicationCards, leaveCode);
        var broughtForward = CalculateBroughtForwardForAnnualLeave(employeeNo, currentYear - 1, leaveApplicationCards);
        var currentBalance = (yearlyEntitlement + broughtForward) - totalTaken;
        var usagePercentage = (yearlyEntitlement + broughtForward) > 0 ? Math.Round((totalTaken / (earnedToDate + broughtForward)) * 100, 1) : 0m;
            
        return new LeaveTypeStatistics
        {
            LeaveCode = leaveCode,
            LeaveDescription = "Annual Leave",
            YearlyEntitlement = yearlyEntitlement,
            EarnedToDate = earnedToDate,
            TotalTaken = totalTaken,
            CurrentBalance = Math.Max(0, currentBalance),
            UsagePercentage = usagePercentage,
            BroughtForward = broughtForward,
            MonthlyTaken = CalculateMonthlyTakenForLeaveType(employeeNo, currentYear, currentMonth, leaveApplicationCards, leaveCode),
            PendingApplications = CalculatePendingApplicationsForLeaveType(employeeNo, currentYear, leaveApplicationCards, leaveCode),
            ApprovedThisMonth = CalculateApprovedThisMonthForLeaveType(employeeNo, currentYear, currentMonth, leaveApplicationCards, leaveCode),
            HasMonthlyAccrual = true,
            MonthlyEarned = 2.5m,
            CurrentPeriod = currentYear.ToString()
        };
    }

    /// <summary>
    /// Backward compatibility method - returns only annual leave statistics
    /// </summary>
    public static AnnualLeaveSummaryResponse CalculateAnnualLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes,
        List<ApprovedLeaves>? approvedLeaves = null)
    {
        var annualStats = CalculateAnnualLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes);

        return new AnnualLeaveSummaryResponse
        {
            TotalEntitlement = annualStats.YearlyEntitlement,
            LeaveEarnedToDate = annualStats.EarnedToDate,
            CurrentBalance = annualStats.CurrentBalance,
            TotalTaken = annualStats.TotalTaken,
            AccumulatedDays = annualStats.BroughtForward,
            UsagePercentage = annualStats.UsagePercentage,
            MonthlyEarned = annualStats.MonthlyEarned ?? 0,
            MonthlyTaken = annualStats.MonthlyTaken,
            MonthlyBalance = (annualStats.MonthlyEarned ?? 0) - annualStats.MonthlyTaken,
            PendingApplications = annualStats.PendingApplications,
            ApprovedThisMonth = annualStats.ApprovedThisMonth,
            CurrentPeriod = annualStats.CurrentPeriod
        };
    }

    /// <summary>
    /// Calculate compassion leave statistics (5 days yearly, no carry forward)
    /// </summary>
    public static LeaveTypeStatistics CalculateCompassionLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes)
    {
        return CalculateStandardLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes, "COMPASSIONATE", "Compassion Leave", 5m);
    }

    public static LeaveTypeStatistics CalculateAdoptionLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes)
    {
        return CalculateStandardLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes, "ADOPTION", "Adoption Leave", 30m);
    }

    /// <summary>
    /// Calculate maternity leave statistics (90 days yearly, no carry forward)
    /// </summary>
    public static LeaveTypeStatistics CalculateMaternityLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes)
    {
        return CalculateStandardLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes, "MATERNITY", "Maternity Leave", 90m);
    }

    /// <summary>
    /// Calculate paternity leave statistics (14 days yearly, no carry forward)
    /// </summary>
    public static LeaveTypeStatistics CalculatePaternityLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes)
    {
        return CalculateStandardLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes, "PATERNITY", "Paternity Leave", 14m);
    }

    /// <summary>
    /// Calculate sick leave statistics (90 days yearly, no carry forward)
    /// </summary>
    public static LeaveTypeStatistics CalculateSickLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes)
    {
        return CalculateStandardLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes, "SICK", "Sick Leave", 90m);
    }

    /// <summary>
    /// Calculate study leave statistics (10 days yearly, no carry forward)
    /// </summary>
    public static LeaveTypeStatistics CalculateStudyLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes)
    {
        return CalculateStandardLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes, "STUDY", "Study Leave", 10m);
    }

    /// <summary>
    /// Calculate unpaid leave statistics (60 days yearly, no carry forward)
    /// </summary>
    public static LeaveTypeStatistics CalculateUnpaidLeaveStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes)
    {
        return CalculateStandardLeaveTypeStatistics(employeeNo, leaveApplicationCards, leaveTypes, "UNPAID", "Unpaid Leave", 60m);
    }

    

    #region Helper Methods

    /// <summary>
    /// Standard calculation for leave types without monthly accrual or carry forward
    /// </summary>
    private static LeaveTypeStatistics CalculateStandardLeaveTypeStatistics(
        string employeeNo,
        List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards,
        List<LeaveTypes> leaveTypes,
        string leaveCode,
        string leaveDescription,
        decimal defaultEntitlement)
    {
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;

        _logger?.LogInformation("Calculating {LeaveType} Statistics for {EmployeeNo}", leaveDescription, employeeNo);

        var leaveType = leaveTypes?.FirstOrDefault(lt => lt.Code?.ToUpperInvariant() == leaveCode);
        var yearlyEntitlement = leaveType?.Days ?? defaultEntitlement;

        var totalTaken = CalculateTotalTakenForLeaveType(employeeNo, currentYear, leaveApplicationCards, leaveCode);
        var currentBalance = yearlyEntitlement - totalTaken;
        var usagePercentage = yearlyEntitlement > 0
            ? Math.Round((totalTaken / yearlyEntitlement) * 100, 1)
            : 0m;

        return new LeaveTypeStatistics
        {
            LeaveCode = leaveCode,
            LeaveDescription = leaveDescription,
            YearlyEntitlement = yearlyEntitlement,
            EarnedToDate = yearlyEntitlement, // For non-accrual leave types, earned = entitlement
            TotalTaken = totalTaken,
            CurrentBalance = Math.Max(0, currentBalance),
            UsagePercentage = usagePercentage,
            BroughtForward = 0, // No carry forward for these leave types
            MonthlyTaken = CalculateMonthlyTakenForLeaveType(employeeNo, currentYear, currentMonth, leaveApplicationCards, leaveCode),
            PendingApplications = CalculatePendingApplicationsForLeaveType(employeeNo, currentYear, leaveApplicationCards, leaveCode),
            ApprovedThisMonth = CalculateApprovedThisMonthForLeaveType(employeeNo, currentYear, currentMonth, leaveApplicationCards, leaveCode),
            HasMonthlyAccrual = false,
            MonthlyEarned = null,
            CurrentPeriod = currentYear.ToString()
        };
    }

    private static decimal GetLeaveEarnedToDate_(string employeeNo, int currentYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards, string leaveCode)
    {
        if (leaveCode != "ANNUAL")
        {
            // For non-annual leave, earned = entitlement (no monthly accrual)
            return 0; // Will be set by calling method
        }

        if (leaveApplicationCards?.Any() != true)
        {
            var currentMonth = DateTime.Now.Month;
            return Math.Max(0, (currentMonth - 1) * 2.5m);
        }

        var earnedToDate = leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          card.Leave_Code == leaveCode)
            .OrderByDescending(card => card.Application_Date)
            .FirstOrDefault()?.Leave_Earned_to_Date ?? 0m;

        if (earnedToDate == 0m && leaveCode == "ANNUAL")
        {
            var currentMonth = DateTime.Now.Month;
            earnedToDate = Math.Max(0, (currentMonth - 1) * 2.5m);
        }

        return earnedToDate;
    }

    private static decimal GetLeaveEarnedToDate(string employeeNo, int currentYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards, string leaveCode)
    {
        if (leaveCode != "ANNUAL")
        {
            return 0; // For non-annual leave types
        }

        // First try to get from database records
        decimal earnedToDate = 0m;
        if (leaveApplicationCards?.Any() == true)
        {
            earnedToDate = leaveApplicationCards
                .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                              card.Employee_No == employeeNo &&
                              card.Leave_Period == currentYear.ToString() &&
                              card.Leave_Code == leaveCode)
                .OrderByDescending(card => card.Application_Date)
                .FirstOrDefault()?.Leave_Earned_to_Date ?? 0m;
        }

        // If no database record found, calculate based on current month
        if (earnedToDate == 0m)
        {
            var currentMonth = DateTime.Now.Month;
            var currentDay = DateTime.Now.Day;

            // More accurate calculation: pro-rate based on day of month
            var monthsCompleted = currentMonth - 1;
            var currentMonthProRated = (currentDay >= 15) ? 0.5m : 0m; // Half month if past 15th

            earnedToDate = Math.Max(0, (monthsCompleted + currentMonthProRated) * 2.5m);
        }

        return earnedToDate;
    }

    private static decimal CalculateTotalTakenForLeaveType(string employeeNo, int currentYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards, string leaveCode)
    {
        if (leaveApplicationCards?.Any() != true) return 0m;

        return leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          card.Status == "Released" &&
                          card.Leave_Code == leaveCode)
            .Sum(card => card.Days_Applied);
    }

    private static decimal CalculateBroughtForwardForAnnualLeave(string employeeNo, int previousYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards)
    {
        if (leaveApplicationCards?.Any() != true) return 0m;

        var previousYearTaken = leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == previousYear.ToString() &&
                          card.Status == "Released" &&
                          card.Leave_Code == "ANNUAL")
            .Sum(card => card.Days_Applied);

        var unutilized = Math.Max(0, 30m - previousYearTaken);
        return Math.Min(unutilized, 10m); // 10-day carry forward limit
    }

    private static decimal CalculateMonthlyTakenForLeaveType(string employeeNo, int currentYear, int currentMonth, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards, string leaveCode)
    {
        if (leaveApplicationCards?.Any() != true) return 0m;

        return leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          card.Start_Date.Year == currentYear &&
                          card.Start_Date.Month == currentMonth &&
                          card.Status == "Released" &&
                          card.Leave_Code == leaveCode)
            .Sum(card => card.Days_Applied);
    }

    private static int CalculatePendingApplicationsForLeaveType(string employeeNo, int currentYear, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards, string leaveCode)
    {
        if (leaveApplicationCards?.Any() != true) return 0;

        string[] pendingStatuses = ["Open", "Pending Approval", "Being Processed"];

        return leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          card.Leave_Code == leaveCode &&
                          pendingStatuses.Contains(card.Leave_Status, StringComparer.OrdinalIgnoreCase))
            .Count();
    }

    private static int CalculateApprovedThisMonthForLeaveType(string employeeNo, int currentYear, int currentMonth, List<Domain.Entities.LeaveApplicationCard> leaveApplicationCards, string leaveCode)
    {
        if (leaveApplicationCards?.Any() != true) return 0;

        return leaveApplicationCards
            .Where(card => !string.IsNullOrEmpty(card.Employee_No) &&
                          card.Employee_No == employeeNo &&
                          card.Leave_Period == currentYear.ToString() &&
                          card.Leave_Code == leaveCode &&
                          card.Start_Date.Year == currentYear &&
                          card.Start_Date.Month == currentMonth &&
                          card.Status == "Released")
            .Count();
    }

    #endregion
}
