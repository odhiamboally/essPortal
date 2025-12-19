using EssPortal.Domain.Enums.NavEnums;
using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

public record LeaveTypeFilter : BaseFilter
{
    public string? Code { get; init; }
    public string? Description { get; init; }
    public int? Days { get; init; }
    public int? AccrueDays { get; init; }
    public decimal? ConversionRatePerDay { get; init; }
    public bool? UnlimitedDays { get; init; }
    public Gender? Gender { get; init; }
    public LeaveType_Balance? Balance { get; init; }
    public int? MaxCarryForwardDays { get; init; }
    public bool? AnnualLeave { get; init; }
    public bool? InclusiveOfHolidays { get; init; }
    public bool? InclusiveOfSaturday { get; init; }
    public bool? InclusiveOfSunday { get; init; }
    public int? OffHolidaysDaysLeave { get; init; }
    public LeaveType_Status? Status { get; init; }



  
}
