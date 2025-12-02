using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

internal record LeavePlannerListFilter : BaseFilter
{
    public string? No { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public DateTime? Date { get; init; }
}
