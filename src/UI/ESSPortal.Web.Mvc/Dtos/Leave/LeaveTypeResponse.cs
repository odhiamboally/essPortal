namespace ESSPortal.Web.Mvc.Dtos.Leave;

public record LeaveTypeResponse
{
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal? Days { get; init; }
    public string? Gender { get; init; }
    public decimal MaxDays { get; init; }
    public bool RequiresApproval { get; init; }
    public bool? AnnualLeave { get; init; }
    public bool IsActive { get; init; } = true;
}