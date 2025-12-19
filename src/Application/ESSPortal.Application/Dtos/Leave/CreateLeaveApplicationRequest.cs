using Microsoft.AspNetCore.Http;

namespace ESSPortal.Application.Dtos.Leave;
public record CreateLeaveApplicationRequest
{
    public string ApplicationNo { get; init; } = string.Empty;
    public DateTime ApplicationDate { get; init; }
    public bool ApplyOnBehalf { get; init; }
    public string EmployeeNo { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public string MobileNo { get; init; } = string.Empty;
    public string LeavePeriod { get; init; } = string.Empty;
    public string LeaveType { get; init; } = string.Empty;
    public decimal DaysApplied { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public DateTime ResumptionDate { get; init; }
    public bool LeaveAllowancePayable { get; init; }
    public List<string> SelectedRelieverEmployeeNos { get; init; } = [];
    public IFormFile[]? Attachments { get; init; }
    public string EmailAddress { get; init; } = string.Empty;
    public string EmploymentType { get; init; } = string.Empty;
    public string ResponsibilityCenter { get; init; } = string.Empty;
    public string DutiesTakenOverBy { get; init; } = string.Empty;
    public string RelievingName { get; init; } = string.Empty;
    public bool HalfDay { get; init; }
    public bool IsEditing { get; set; }
}
