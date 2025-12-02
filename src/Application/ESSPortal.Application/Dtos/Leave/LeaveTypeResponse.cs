namespace ESSPortal.Application.Dtos.Leave;
public record LeaveTypeResponse
{
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal MaxDays { get; init; } // For validation
    public bool RequiresApproval { get; init; } // Business rule
    public decimal? Days { get; internal set; }
    public string? Gender { get; internal set; }
    public bool? AnnualLeave { get; internal set; }
}