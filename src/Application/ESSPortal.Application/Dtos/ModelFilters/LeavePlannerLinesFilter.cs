using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

public record LeavePlannerLinesFilter : BaseFilter
{
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public string? LeaveType { get; init; }
    public decimal? NoOfDays { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime? ResumptionDate { get; init; }
}
