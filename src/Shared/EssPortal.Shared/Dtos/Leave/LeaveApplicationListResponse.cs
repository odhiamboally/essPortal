
namespace ESSPortal.Shared.Dtos.Leave;

public record LeaveApplicationListResponse
{
    public string ApplicationNo { get; init; } = string.Empty;
    public DateTime ApplicationDate { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public string LeaveType { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal DaysApplied { get; init; }
    public string? Status { get; init; }
    public string LeavePeriod { get; init; } = string.Empty;

    // Calculated properties
    public int Duration => (EndDate - StartDate).Days + 1;
    public bool IsCurrentYear => StartDate.Year == DateTime.Now.Year;
    public bool IsApproved => Status == "Released";
    public bool IsPending => Status == "Open" || Status == "Pending Approval";
        
    public bool IsRejected => Status == "Rejected";

    // Computed properties for display
    public string DurationText => $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";
    public string StatusDisplayText => Status!.Replace("_", " ");
    public string StatusCssClass => Status switch
    {
        "Released" => "status-approved", "Open" or "Pending Approval" => "status-pending",
        "Rejected" => "status-rejected",
        _ => "status-pending"
    };
    public string StateCssClass => Status switch
    {
        "Released" => "state-completed",
        "Open" or "Pending Approval" => "state-review",
        "Rejected" => "state-closed",
        _ => "state-review"
    };
    public string StateText => Status switch
    {
        "Released" => "Completed",
        "Open" or "Pending Approval" => "Under Review",
        "Rejected" => "Closed",
        _ => "Under Review"
    };
}
