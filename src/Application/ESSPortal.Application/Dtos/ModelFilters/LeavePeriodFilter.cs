using EssPortal.Domain.Enums.NavEnums;
using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

public record LeavePeriodFilter : BaseFilter
{
    public string? LeavePeriod { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool? Closed { get; init; }
    public string? LeaveType { get; init; }
    public Employment_Type? EmploymentType { get; init; }
    public string? EmployeeNo { get; init; }
}
