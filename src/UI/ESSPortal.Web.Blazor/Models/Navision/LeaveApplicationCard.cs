using EssPortal.Web.Blazor.Enums.NavEnums;

namespace EssPortal.Web.Blazor.Models.Navision;

public partial class LeaveApplicationCard
{
    public string? Key { get; set; }
    public string? Application_No { get; set; }
    public DateTime Application_Date { get; set; }
    public bool Application_DateSpecified { get; set; }
    public bool Apply_on_behalf { get; set; }
    public bool Apply_on_behalfSpecified { get; set; }
    public string? Employee_No { get; set; }
    public string? Employee_Name { get; set; }
    public string? Email_Adress { get; set; }
    public Employment_Type? Employment_Type { get; set; }
    public bool Employment_TypeSpecified { get; set; }
    public string? Responsibility_Center { get; set; }
    public string? Mobile_No { get; set; }
    public string? Shortcut_Dimension_1_Code { get; set; }
    public string? Shortcut_Dimension_2_Code { get; set; }
    public string? Leave_Period { get; set; }
    public string? Leave_Code { get; set; }
    public Leave_Status Leave_Status { get; set; }
    public bool Leave_StatusSpecified { get; set; }
    public LeaveApplicationCardStatus Status { get; set; }
    public bool StatusSpecified { get; set; }
    public decimal Leave_Earned_to_Date { get; set; }
    public bool Leave_Earned_to_DateSpecified { get; set; }
    public decimal Days_Applied { get; set; }
    public bool Days_AppliedSpecified { get; set; }
    public DateTime Start_Date { get; set; }
    public bool Start_DateSpecified { get; set; }
    public DateTime End_Date { get; set; }
    public bool End_DateSpecified { get; set; }
    public DateTime Resumption_Date { get; set; }
    public bool Resumption_DateSpecified { get; set; }
    public string? Duties_Taken_Over_By { get; set; }
    public string? Relieving_Name { get; set; }
    public bool Leave_Allowance_Payable { get; set; }
    public bool Leave_Allowance_PayableSpecified { get; set; }
    public List<LeaveReliever> Relievers { get; set; } = [];
}
