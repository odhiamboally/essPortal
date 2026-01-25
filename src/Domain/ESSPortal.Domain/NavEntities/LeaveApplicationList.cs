using EssPortal.Domain.Enums.NavEnums;

namespace ESSPortal.Domain.NavEntities;

public class LeaveApplicationList
{
    public string Application_No { get; set; } = string.Empty;
    public DateTime Application_Date { get; set; }
    public string? Employee_No { get; set; }
    public string? Employee_Name { get; set; }
    public decimal? Days_Applied { get; set; }
    public DateTime Start_Date { get; set; }
    public DateTime End_Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Leave_Period { get; set; }
    public string? Key { get; set; }
    
    public bool ApplicationDateSpecified { get; set; }
    
    public bool DaysAppliedSpecified { get; set; }
    
    public bool StartDateSpecified { get; set; }
    
    public bool EndDateSpecified { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public bool StatusSpecified { get; set; }
    public string? LeavePeriod { get; set; }
}
