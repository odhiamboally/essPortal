namespace EssPortal.Shared.Dtos.ModelFilters;

public record LeaveApplicationListFilter : BaseFilter
{
    public string? ApplicationNo { get; init; }
    public DateTimeOffset? ApplicationDate { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public decimal? DaysApplied { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? Status { get; init; }
    public string? LeavePeriod { get; init; }

}
