namespace ESSPortal.Web.Mvc.Models.Navision;

public class ApprovedLeave
{
    public string Application_No { get; set; } = string.Empty;
    public DateTime Application_Date { get; set; }
    public string? Employee_No { get; set; }
    public string? Employee_Name { get; set; }
    public string? Leave_Code { get; set; }
    public decimal Days_Applied { get; set; }
    public DateTime Start_Date { get; set; }
    public DateTime End_Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Email_Adress { get; set; }
}
