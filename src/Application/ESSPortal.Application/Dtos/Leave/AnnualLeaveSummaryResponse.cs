namespace ESSPortal.Application.Dtos.Leave;

public record AnnualLeaveSummaryResponse
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

    public string CurrentPeriod { get; init; } = DateTime.Now.Year.ToString();

    // Display helpers
    public bool HasLeaveData => TotalEntitlement > 0 || CurrentBalance > 0 || TotalTaken > 0;
    public bool IsOverAllocated => TotalTaken > LeaveEarnedToDate;
    public bool IsLowBalance => CurrentBalance <= 5;
    public bool IsHighUsage => UsagePercentage >= 75;

    public string CurrentMonth => DateTime.Now.ToString("MMMM yyyy");
    public string BalanceDisplayClass => IsLowBalance ? "text-warning" : "text-success";
    public string UsageDisplayClass => UsagePercentage >= 80 ? "text-danger" :
                                      UsagePercentage >= 60 ? "text-warning" : "text-success";
    public string UsageCategory => UsagePercentage switch
    {
        >= 90 => "critical",
        >= 75 => "high",
        >= 50 => "moderate",
        _ => "low"
    };


}
