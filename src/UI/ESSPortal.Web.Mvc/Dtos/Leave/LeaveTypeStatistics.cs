namespace ESSPortal.Web.Mvc.Dtos.Leave;

public record LeaveTypeStatistics
{
    public string LeaveCode { get; init; } = string.Empty;
    public string LeaveDescription { get; init; } = string.Empty;
    public decimal YearlyEntitlement { get; init; }
    public decimal EarnedToDate { get; init; }
    public decimal TotalTaken { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal UsagePercentage { get; init; }
    public decimal BroughtForward { get; init; }
    public decimal MonthlyTaken { get; init; }
    public int PendingApplications { get; init; }
    public int ApprovedThisMonth { get; init; }
    public bool HasMonthlyAccrual { get; init; } // Only annual leave
    public decimal? MonthlyEarned { get; init; } // Only for annual leave
    public string CurrentPeriod { get; init; } = string.Empty;

    public string CurrentBalanceText => $"{CurrentBalance:F1} days";
    public string UsagePercentageText => $"{UsagePercentage:F1}%";
    public string BalanceDisplayClass => CurrentBalance <= 5 ? "text-warning" : "text-success";
    public string UsageDisplayClass => UsagePercentage >= 80 ? "text-danger" :
                                      UsagePercentage >= 60 ? "text-warning" : "text-success";



}