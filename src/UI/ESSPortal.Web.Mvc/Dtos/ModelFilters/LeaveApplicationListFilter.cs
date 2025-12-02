using EssPortal.Web.Mvc.Enums.NavEnums;
namespace EssPortal.Web.Mvc.Dtos.ModelFilters;

public record LeaveApplicationListFilter //: BaseFilter
{
    public string? ApplicationNo { get; init; }
    public DateTimeOffset? ApplicationDate { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public decimal? DaysApplied { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public LeaveApplicationListStatus? Status { get; init; }
    public string? LeavePeriod { get; init; }

    public Dictionary<string, string?> CustomQueryParameters()
    {
        var parameters = new Dictionary<string, string?>();

        void AddIf(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                parameters[key] = value;
        }

        AddIf(nameof(ApplicationNo), ApplicationNo);
        AddIf(nameof(ApplicationDate), ApplicationDate?.ToString("yyyy-MM-dd"));
        AddIf(nameof(EmployeeNo), EmployeeNo);
        AddIf(nameof(EmployeeName), EmployeeName);
        AddIf(nameof(DaysApplied), DaysApplied?.ToString());
        AddIf(nameof(StartDate), StartDate?.ToString("yyyy-MM-dd"));
        AddIf(nameof(EndDate), EndDate?.ToString("yyyy-MM-dd"));
        AddIf(nameof(Status), Status?.ToString());
        AddIf(nameof(LeavePeriod), LeavePeriod);

        return parameters;
    }
}
