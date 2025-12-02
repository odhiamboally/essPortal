using EssPortal.Web.Mvc.Enums.NavEnums;

namespace EssPortal.Web.Mvc.Dtos.ModelFilters;

public record LeaveApplicationCardFilter
{
    public string? Application_No { get; init; }
    public DateTimeOffset? ApplicationDate { get; init; }
    public bool? Applyonbehalf { get; init; }
    public string? Employee_No { get; init; }
    public string? Employee_Name { get; init; }
    public string? EmailAdress { get; init; }
    public Employment_Type? EmploymentType { get; init; }
    public string? ResponsibilityCenter { get; init; }
    public string? MobileNo { get; init; }
    public string? ShortcutDimension1Code { get; init; }
    public string? ShortcutDimension2Code { get; init; }
    public string? LeavePeriod { get; init; }
    public string? LeaveCode { get; init; }
    public Leave_Status? LeaveStatus { get; init; }
    public LeaveApplicationCardStatus? Status { get; init; }
    public decimal? LeaveEarnedtoDate { get; init; }
    public decimal? DaysApplied { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public DateTimeOffset? ResumptionDate { get; init; }
    public string? DutiesTakenOverBy { get; init; }
    public string? RelievingName { get; init; }
    public bool? LeaveAllowancePayable { get; init; }



    public Dictionary<string, string?> CustomQueryParameters()
    {
        var parameters = new Dictionary<string, string?>();

        void AddIf(string key, object? value)
        {
            if (value is null) return;

            string? stringValue = value switch
            {
                DateTimeOffset date => date.ToString("yyyy-MM-dd"),
                bool b => b.ToString(),
                Enum e => e.ToString(),
                _ => value.ToString()
            };

            if (!string.IsNullOrWhiteSpace(stringValue))
                parameters[key] = stringValue;
        }

        AddIf(nameof(Application_No), Application_No);
        AddIf(nameof(ApplicationDate), ApplicationDate);
        AddIf(nameof(Applyonbehalf), Applyonbehalf);
        AddIf(nameof(Application_No), Application_No);
        AddIf(nameof(Application_No), Application_No);
        AddIf(nameof(EmailAdress), EmailAdress);
        AddIf(nameof(EmploymentType), EmploymentType);
        AddIf(nameof(ResponsibilityCenter), ResponsibilityCenter);
        AddIf(nameof(MobileNo), MobileNo);
        AddIf(nameof(ShortcutDimension1Code), ShortcutDimension1Code);
        AddIf(nameof(ShortcutDimension2Code), ShortcutDimension2Code);
        AddIf(nameof(LeavePeriod), LeavePeriod);
        AddIf(nameof(LeaveCode), LeaveCode);
        AddIf(nameof(LeaveStatus), LeaveStatus);
        AddIf(nameof(Status), Status);
        AddIf(nameof(LeaveEarnedtoDate), LeaveEarnedtoDate);
        AddIf(nameof(DaysApplied), DaysApplied);
        AddIf(nameof(StartDate), StartDate);
        AddIf(nameof(EndDate), EndDate);
        AddIf(nameof(ResumptionDate), ResumptionDate);
        AddIf(nameof(DutiesTakenOverBy), DutiesTakenOverBy);
        AddIf(nameof(RelievingName), RelievingName);
        AddIf(nameof(LeaveAllowancePayable), LeaveAllowancePayable);

        return parameters;
    }
}
