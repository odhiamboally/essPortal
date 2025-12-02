namespace EssPortal.Web.Mvc.Models.Navision;

public class LeavePlannerLine
{
    public string? Key { get; set; }
    public string? EmployeeNo { get; set; }
    public string? EmployeeName { get; set; }
    public string? LeaveType { get; set; }
    public decimal NumberOfDays { get; set; }
    public bool NumberOfDaysSpecified { get; set; }
    public DateTime StartDate { get; set; }
    public bool StartDateSpecified { get; set; }
    public DateTime EndDate { get; set; }
    public bool EndDateSpecified { get; set; }
    public DateTime ResumptionDate { get; set; }
    public bool ResumptionDateSpecified { get; set; }
}
