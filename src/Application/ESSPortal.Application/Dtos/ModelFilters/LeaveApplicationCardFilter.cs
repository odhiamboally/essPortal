using EssPortal.Domain.Enums.NavEnums;
using ESSPortal.Application.Dtos.ModelFilters;
namespace EssPortal.Application.Dtos.ModelFilters;

public record LeaveApplicationCardFilter : BaseFilter
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



   
}
