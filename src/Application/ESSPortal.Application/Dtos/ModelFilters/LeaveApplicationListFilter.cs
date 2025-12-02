using EssPortal.Domain.Enums.NavEnums;
using ESSPortal.Application.Dtos.ModelFilters;
namespace EssPortal.Application.Dtos.ModelFilters;

public record LeaveApplicationListFilter : BaseFilter
{
    public string Application_No { get; init; } = string.Empty;
    public DateTimeOffset? ApplicationDate { get; init; }
    public string Employee_No { get; init; } = string.Empty;
    public string Employee_Name { get; init; } = string.Empty;
    public decimal? DaysApplied { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public LeaveApplicationListStatus? Status { get; init; }
    public string LeavePeriod { get; init; } = string.Empty;
}
