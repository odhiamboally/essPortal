namespace ESSPortal.Web.Mvc.Dtos.Leave;

public class AnnualLeaveSummaryResponse
{
    public decimal TotalEntitlement { get; init; } = 30;
    public decimal LeaveEarnedToDate { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal TotalTaken { get; init; }
    public decimal AccumulatedDays { get; init; }
    public decimal UsagePercentage { get; init; }

    public decimal MonthlyEarned { get; init; } = 2.5m;
    public decimal MonthlyTaken { get; init; }
    public decimal MonthlyBalance { get; init; }

    public decimal PendingApplications { get; init; }
    public decimal ApprovedThisMonth { get; init; }

    public string CurrentPeriod { get; init; } = string.Empty;

    // Display helpers
    public bool HasLeaveData { get; init; }
    public bool IsOverAllocated { get; init; }
    public bool IsLowBalance { get; init; }
    public bool IsHighUsage { get; init; }

    public string CurrentMonth { get; init; } = string.Empty;
    public string BalanceDisplayClass { get; init; } = string.Empty;
    public string UsageDisplayClass { get; init; } = string.Empty;
    public string UsageCategory { get; init; } = string.Empty;

    
}
