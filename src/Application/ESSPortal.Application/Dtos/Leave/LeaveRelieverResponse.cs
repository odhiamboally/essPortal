namespace ESSPortal.Application.Dtos.Leave;
public record LeaveRelieverResponse
{
    public string EmployeeNo { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public string EmailAddress { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
}