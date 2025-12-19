using EssPortal.Domain.Enums.NavEnums;

namespace ESSPortal.Application.Dtos.Leave;
public record LeaveApplicationCardResponse
{
    public string ApplicationNo { get; init; } = string.Empty;
    public DateTime? ApplicationDate { get; init; }
    public bool ApplyOnBehalf { get; init; }
    public bool ApplyOnBehalfSpecified { get; init; }

    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public string? EmailAddress { get; init; }
    public string EmploymentType { get; init; } = string.Empty;
    public string? ResponsibilityCenter { get; init; }
    public string? MobileNo { get; init; }
    public string? ShortcutDimension1Code { get; init; }
    public string? ShortcutDimension2Code { get; init; }

    public string? LeavePeriod { get; init; }
    public string? LeaveCode { get; init; }
    public string? LeaveStatus { get; init; }
    public LeaveApplicationCardStatus Status { get; init; }
    public decimal LeaveEarnedToDate { get; init; }
    public decimal DaysApplied { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime ResumptionDate { get; init; }
    public string? DutiesTakenOverBy { get; init; }
    public string? RelievingName { get; init; }
    public bool LeaveAllowancePayable { get; init; }
    public bool LeaveAllowancePayableSpecified { get; init; }

    // Calculated properties
    public LeaveApplicationCardStatus EffectiveStatus => Status;
    public bool IsApproved => Status == LeaveApplicationCardStatus.Released;
    public bool IsPending => Status == LeaveApplicationCardStatus.Open || Status == LeaveApplicationCardStatus.Pending_Approval;
    public bool IsRejected => Status == LeaveApplicationCardStatus.Rejected;
    public int Duration => (EndDate - StartDate).Days + 1;
    public string StatusDisplayText => Status.ToString().Replace("_", " ");
    public string DurationText => $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";
}