namespace ESSPortal.Application.Dtos.Leave;
public record LeaveHistoryResponse
{
    public string ApplicationNo { get; init; } = string.Empty;
    public DateTime ApplicationDate { get; init; }
    public string LeaveType { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal DaysApplied { get; init; }
    public string DutiesTakenOverBy { get; internal set; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string LeavePeriod { get; init; } = string.Empty;

    // Computed properties for dashboard display
    public string DurationText { get; init; } = string.Empty;
    public string StatusDisplayText { get; init; } = string.Empty;
    public string StatusCssClass { get; init; } = string.Empty;
    public string StateText { get; init; } = string.Empty;
    public string StateCssClass { get; init; } = string.Empty;
    public bool IsCurrentYear { get; init; }
    public bool IsApproved { get; init; }
    public bool IsPending { get; init; }
    public bool IsRejected { get; init; }
    public int Duration { get; internal set; }
    
}