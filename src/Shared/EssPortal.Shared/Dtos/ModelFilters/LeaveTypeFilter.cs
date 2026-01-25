
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



    public Dictionary<string, string?> CustomQueryParameters()
    {
        var parameters = new Dictionary<string, string?>();

        void AddIf(string key, string? value)
        {
           if (!string.IsNullOrWhiteSpace(value))
              parameters[key] = value;
        }

        AddIf(nameof(Code), Code);
        AddIf(nameof(Description), Description);
        AddIf(nameof(Days), Days.HasValue ? Days.Value.ToString() : string.Empty);
        AddIf(nameof(AccrueDays), AccrueDays.HasValue ? AccrueDays.Value.ToString() : string.Empty);
        AddIf(nameof(ConversionRatePerDay), ConversionRatePerDay.HasValue ? ConversionRatePerDay.Value.ToString() : string.Empty);
        AddIf(nameof(UnlimitedDays), UnlimitedDays.HasValue ? UnlimitedDays.Value.ToString() : string.Empty);
        AddIf(nameof(Gender), Gender);
        AddIf(nameof(Balance), Balance ?? string.Empty);
        AddIf(nameof(MaxCarryForwardDays), MaxCarryForwardDays.HasValue ? MaxCarryForwardDays.Value.ToString() : string.Empty);
        AddIf(nameof(AnnualLeave), AnnualLeave.HasValue ? AnnualLeave.Value.ToString() : string.Empty);
        AddIf(nameof(InclusiveOfHolidays), InclusiveOfHolidays.HasValue ? InclusiveOfHolidays.Value.ToString() : string.Empty);
        AddIf(nameof(InclusiveOfSaturday), InclusiveOfSaturday.HasValue ? InclusiveOfSaturday.Value.ToString() : string.Empty);
        AddIf(nameof(InclusiveOfSunday), InclusiveOfSunday.HasValue ? InclusiveOfSunday.Value.ToString() : string.Empty);
        AddIf(nameof(OffHolidaysDaysLeave), OffHolidaysDaysLeave.HasValue ? OffHolidaysDaysLeave.Value.ToString() : string.Empty);
        AddIf(nameof(Status), Status);

        return parameters;
    }
}
