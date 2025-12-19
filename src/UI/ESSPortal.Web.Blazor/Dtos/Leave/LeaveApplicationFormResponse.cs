using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Web.Blazor.Dtos.Leave;

public record LeaveApplicationFormResponse
{
    public string ApplicationNo { get; set; } = string.Empty;
    public LeaveApplicationEmployeeResponse Employee { get; init; } = new();
    public AnnualLeaveSummaryResponse AnnualLeaveSummary { get; init; } = new();
    public LeaveSummaryResponse LeaveSummary { get; init; } = new();
    public List<LeaveTypeResponse> LeaveTypes { get; init; } = [];
    public List<LeaveRelieverResponse> AvailableRelievers { get; init; } = [];

    public EditLeaveData? ExistingLeaveData { get; init; }
}
