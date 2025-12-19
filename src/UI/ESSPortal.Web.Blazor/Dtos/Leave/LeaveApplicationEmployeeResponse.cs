namespace ESSPortal.Web.Blazor.Dtos.Leave;

public record LeaveApplicationEmployeeResponse
{
    public string EmployeeNo { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public string EmailAddress { get; init; } = string.Empty;
    public string ResponsibilityCenter { get; init; } = string.Empty;
    public string BranchCode { get; init; } = string.Empty;
    public string MobileNo { get; init; } = string.Empty; // Allow editing
}
