using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

public record LeaveStatisticsFactboxFilter : BaseFilter
{
    public string? EmployeeNumber { get; init; }
    public string? LeaveEntitlment { get; init; }
    public string? LeaveEarnedToDate { get; init; }
    public string? RecalledDays { get; init; }
    public string? DaysAbsent { get; init; }
    public string? BalanceBroughtForward { get; init; }
    public string? TotalLeaveDaysTaken { get; init; }
    public string? LeaveBalance { get; init; }
}
