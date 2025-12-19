namespace ESSPortal.Web.Blazor.ViewModels.Leave;


/// <summary>
/// Individual row in the leave statistics table
/// </summary>
public class LeaveTypeRowViewModel
{
    public string LeaveType { get; set; } = string.Empty;
    public string LeaveCode { get; set; } = string.Empty;
    public decimal AnnualEntitlement { get; set; }
    public decimal BroughtForward { get; set; }
    public decimal Taken { get; set; }
    public decimal TotalBalance { get; set; }

    // Display properties
    public string AnnualEntitlementText => $"{AnnualEntitlement:F1}";
    public string BroughtForwardText => BroughtForward > 0 ? $"{BroughtForward:F1}" : "-";
    public string TakenText => Taken > 0 ? $"{Taken:F1}" : "-";
    public string TotalBalanceText => $"{TotalBalance:F1}";

    // Status indicators
    public bool IsLowBalance => TotalBalance <= 5 && AnnualEntitlement > 0;
    public bool HasBroughtForward => BroughtForward > 0;
    public bool HasBeenUsed => Taken > 0;
    public decimal UsagePercentage => AnnualEntitlement > 0 ? Math.Round(Taken / AnnualEntitlement * 100, 1) : 0;

    // CSS classes for styling
    public string BalanceDisplayClass => IsLowBalance ? "text-warning fw-bold" : "text-success";
    public string TakenDisplayClass => HasBeenUsed ? "text-info" : "text-muted";
    public string BroughtForwardDisplayClass => HasBroughtForward ? "text-success" : "text-muted";

    // Row styling
    public string RowCssClass => IsLowBalance ? "table-warning" : "";

    // Progress indicators
    public int UsageProgressValue => (int)Math.Min(100, Math.Max(0, UsagePercentage));
    public string UsageProgressClass => UsagePercentage >= 80 ? "bg-danger" :
                                       UsagePercentage >= 60 ? "bg-warning" : "bg-success";
}
