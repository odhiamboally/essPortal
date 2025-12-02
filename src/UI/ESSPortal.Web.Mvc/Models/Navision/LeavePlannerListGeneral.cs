namespace EssPortal.Web.Mvc.Models.Navision;

public class LeavePlannerListGeneral
{
    public string? Key { get; set; }
    public string? No { get; set; }
    public string? EmployeeNo { get; set; }
    public string? EmployeeName { get; set; }
    public DateTimeOffset Date { get; set; }
    public bool DateSpecified { get; set; }
    public bool Submitted { get; set; }
    public bool SubmittedSpecified { get; set; }
}
