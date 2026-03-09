
namespace EssPortal.Shared.Dtos.ModelFilters;

public record LeaveTypeFilter : BaseFilter
{
    public string? Code { get; init; }
    public string? Description { get; init; }
    public int? Days { get; init; }
    public int? AccrueDays { get; init; }
    public decimal? ConversionRatePerDay { get; init; }
    public bool? UnlimitedDays { get; init; }
    public string? Gender { get; init; }
    public string? Balance { get; init; }
    public int? MaxCarryForwardDays { get; init; }
    public bool? AnnualLeave { get; init; }
    public bool? InclusiveOfHolidays { get; init; }
    public bool? InclusiveOfSaturday { get; init; }
    public bool? InclusiveOfSunday { get; init; }
    public int? OffHolidaysDaysLeave { get; init; }
    public string? Status { get; init; }


}
