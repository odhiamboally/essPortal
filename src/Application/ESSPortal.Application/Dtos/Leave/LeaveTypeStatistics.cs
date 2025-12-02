namespace ESSPortal.Application.Dtos.Leave;
public class LeaveTypeStatistics
{
    public string LeaveCode { get; set; } = string.Empty;
    public string LeaveDescription { get; set; } = string.Empty;
    public decimal YearlyEntitlement { get; set; }
    public decimal EarnedToDate { get; set; }
    public decimal TotalTaken { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal UsagePercentage { get; set; }
    public decimal BroughtForward { get; set; }
    public decimal MonthlyTaken { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedThisMonth { get; set; }
    public bool HasMonthlyAccrual { get; set; } // Only annual leave has monthly accrual
    public decimal? MonthlyEarned { get; set; } // Only for annual leave
    public string CurrentPeriod { get; set; } = string.Empty;
}
