namespace ESSPortal.Domain.NavEntities;

public class LeavePlannerLines
{
    public string? Document_No { get; set; }
    public int Line_No { get; set; }
    public string? Employee_No { get; set; }
    public string? Employee_Name { get; set; }
    public string? Responsibility_Center { get; set; }
    public string? Leave_Type { get; set; }
    public decimal? No_of_Days { get; set; }
    public DateTime? Start_Date { get; set; }
    public DateTime? End_Date { get; set; }
    public DateTime? Resumption_Date { get; set; }
}
