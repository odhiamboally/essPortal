using EssPortal.Web.Mvc.Enums.NavEnums;

namespace EssPortal.Web.Mvc.Models.Navision;

public class LeaveApplicationList
{
    public string? Key { get; set; }
    public string? Application_No { get; set; }
    public DateTime Application_Date { get; set; }
    public bool ApplicationDateSpecified { get; set; }
    public string? Employee_No { get; set; }
    public string? Employee_Name { get; set; }
    public decimal Days_Applied { get; set; }
    public bool DaysAppliedSpecified { get; set; }
    public DateTime Start_Date { get; set; }
    public bool StartDateSpecified { get; set; }
    public DateTime End_Date { get; set; }
    public bool EndDateSpecified { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public LeaveApplicationListStatus Status { get; set; }
    public bool StatusSpecified { get; set; }
    public string? LeavePeriod { get; set; }


}
