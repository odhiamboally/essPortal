using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

internal record LeavePlannerListGeneralFilter : BaseFilter
{
    public string? No { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public string? Date { get; init; }
    public string? Submitted { get; init; }
}
