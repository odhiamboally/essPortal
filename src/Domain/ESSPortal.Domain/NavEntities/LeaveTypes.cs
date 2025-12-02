namespace ESSPortal.Domain.NavEntities;

public class LeaveTypes
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Days { get; set; }
    public bool? Accrue_Days { get; set; }
    public decimal? Conversion_Rate_Per_Day { get; set; }
    public bool? Unlimited_Days { get; set; }
    public string? Gender { get; set; }
    public string? Balance { get; set; }
    public decimal? Max_Carry_Forward_Days { get; set; }
    public bool? Annual_Leave { get; set; }
    public bool? Inclusive_of_Holidays { get; set; }
    public bool? Inclusive_of_Saturday { get; set; }
    public bool? Inclusive_of_Sunday { get; set; }
    public bool? Off_Holidays_Days_Leave { get; set; }
    public string? Status { get; set; }
}
