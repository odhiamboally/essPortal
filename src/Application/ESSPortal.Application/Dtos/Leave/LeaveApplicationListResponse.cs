using EssPortal.Domain.Enums.NavEnums;

namespace ESSPortal.Application.Dtos.Leave;
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
    public LeaveApplicationListStatus Status { get; init; }
    public string LeavePeriod { get; init; } = string.Empty;

    // Calculated properties
    public int Duration => (EndDate - StartDate).Days + 1;
    public bool IsCurrentYear => StartDate.Year == DateTime.Now.Year;
    public bool IsApproved => Status == LeaveApplicationListStatus.Released;
    public bool IsPending => Status == LeaveApplicationListStatus.Open || Status == LeaveApplicationListStatus.Pending_Approval;
    public bool IsRejected => Status == LeaveApplicationListStatus.Rejected;

    // Computed properties for display
    public string DurationText => $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";
    public string StatusDisplayText => Status.ToString().Replace("_", " ");
    public string StatusCssClass => Status switch
    {
        LeaveApplicationListStatus.Released => "status-approved",
        LeaveApplicationListStatus.Open or LeaveApplicationListStatus.Pending_Approval => "status-pending",
        LeaveApplicationListStatus.Rejected => "status-rejected",
        _ => "status-pending"
    };
    public string StateCssClass => Status switch
    {
        LeaveApplicationListStatus.Released => "state-completed",
        LeaveApplicationListStatus.Open or LeaveApplicationListStatus.Pending_Approval => "state-review",
        LeaveApplicationListStatus.Rejected => "state-closed",
        _ => "state-review"
    };
    public string StateText => Status switch
    {
        LeaveApplicationListStatus.Released => "Completed",
        LeaveApplicationListStatus.Open or LeaveApplicationListStatus.Pending_Approval => "Under Review",
        LeaveApplicationListStatus.Rejected => "Closed",
        _ => "Under Review"
    };
}