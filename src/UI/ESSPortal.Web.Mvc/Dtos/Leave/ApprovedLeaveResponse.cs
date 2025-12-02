namespace ESSPortal.Web.Mvc.Dtos.Leave;

public record ApprovedLeaveResponse
{
    public string ApplicationNo { get; init; } = string.Empty;
    public DateTime ApplicationDate { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public string? LeaveCode { get; init; }
    public decimal DaysApplied { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? EmailAddress { get; init; }

    // Computed properties
    public int Duration { get; init; }
    public string DurationText { get; init; } = string.Empty;
    public bool IsCurrentYear { get; init; }
    public int Year => StartDate.Year;
}
