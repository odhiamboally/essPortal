namespace ESSPortal.Application.Dtos.Leave;
public record LeaveTypeResponse
{
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? Days { get; init; }
    public decimal MaxDays { get; init; } // For validation
    public bool? AccrueDays { get; init; }
    public decimal? ConversionRatePerDay { get; init; }
    public bool? UnlimitedDays { get; init; }
    public string? Gender { get; init; }
    public string? Balance { get; init; }
    public decimal? MaxCarryForwardDays { get; init; }
    public bool? AnnualLeave { get; init; }
    public bool? InclusiveOfHolidays { get; init; }
    public bool? InclusiveOfSaturday { get; init; }
    public bool? InclusiveOfSunday { get; init; }
    public bool? OffHolidaysDaysLeave { get; init; }
    public string? Status { get; init; }
    public bool RequiresApproval { get; init; } // Business rule
}