namespace ESSPortal.Domain.NavEntities;

public class LeaveApplicationCard
{
    public string Application_No { get; set; } = string.Empty;
    public bool? Apply_on_behalf { get; set; }
    public string? Employee_No { get; set; }
    public string? Employment_Type { get; set; }
    public string? Responsibility_Center { get; set; }
    public string? Mobile_No { get; set; }
    public string? Shortcut_Dimension_1_Code { get; set; }
    public string? Shortcut_Dimension_2_Code { get; set; }
    public string? Leave_Period { get; set; }
    public string? Leave_Code { get; set; }
    public string? Leave_Status { get; set; }
    public string? Status { get; set; }
    public decimal? Leave_Earned_to_Date { get; set; }
    public decimal Days_Applied { get; set; }
    public DateTime Start_Date { get; set; }
    public string? Duties_Taken_Over_By { get; set; }
    public bool? Leave_Allowance_Payable { get; set; }
}
