namespace ESSPortal.Web.Blazor.Configurations;

public class LeaveTypeConfiguration
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? MaxDays { get; set; }
    public bool UsesCalendarDays { get; set; } 
    public bool RequiresEarnedDays { get; set; } // Only annual leave
    public bool AllowsCarryForward { get; set; } 
    public string[] RequiredGenders { get; set; } = [];
}