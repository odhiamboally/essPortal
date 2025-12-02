using EssPortal.Domain.Enums.NavEnums;
using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

internal record LeaveApplicationListGeneralFilter : BaseFilter
{
    public string? ApplicationNo { get; init; }
    public string? ApplicationDate { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public int? DaysApplied { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public string? LeavePeriod { get; init; }
    public LeaveApplicationListStatus? Status { get; init; }
}
