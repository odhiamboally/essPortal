namespace ESSPortal.Web.Mvc.ViewModels.Leave;

public class LeaveSummaryViewModel
{
    public List<LeaveTypeRowViewModel> LeaveTypeRows { get; set; } = [];

    // Summary totals (if needed)
    public decimal TotalEntitlements => LeaveTypeRows.Sum(r => r.AnnualEntitlement);
    public decimal TotalBroughtForward => LeaveTypeRows.Sum(r => r.BroughtForward);
    public decimal TotalTaken => LeaveTypeRows.Sum(r => r.Taken);
    public decimal TotalBalance => LeaveTypeRows.Sum(r => r.TotalBalance);

    // Display helpers
    public bool HasAnyLeave => LeaveTypeRows.Any(r => r.TotalBalance > 0);
    public bool HasAnyTaken => LeaveTypeRows.Any(r => r.Taken > 0);
    public List<LeaveTypeRowViewModel> AvailableLeaveTypes => LeaveTypeRows.Where(r => r.TotalBalance > 0).ToList();
    public List<LeaveTypeRowViewModel> LowBalanceLeaveTypes => LeaveTypeRows.Where(r => r.IsLowBalance).ToList();

}
