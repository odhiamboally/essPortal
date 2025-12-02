using System.Text.Json.Serialization;

namespace ESSPortal.Domain.NavEntities.LeaveApplication;

public class LeaveApplicationCard
{
    public string? applicationNo { get; set; }
    public string? employeeNumber { get; set; }
    public bool? applyOnBehalf { get; set; }
    public string? leaveCode { get; set; }


    [JsonPropertyName("leaveStartDate")]
    public DateOnly leaveStartDate { get; set; }
    public decimal daysApplied { get; set; }
    public string? dutiesTakenOverBy { get; set; }
}
