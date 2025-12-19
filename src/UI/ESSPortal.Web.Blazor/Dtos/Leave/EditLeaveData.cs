namespace ESSPortal.Web.Blazor.Dtos.Leave;

public record EditLeaveData
{
    public string LeaveType { get; init; } = string.Empty;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public decimal DaysApplied { get; init; }
    public bool HalfDay { get; init; }
    public bool LeaveAllowancePayable { get; init; }
    public string SelectedRelieverEmployeeNo { get; init; } = string.Empty;
}