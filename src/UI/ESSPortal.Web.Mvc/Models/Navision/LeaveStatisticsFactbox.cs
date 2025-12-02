namespace EssPortal.Web.Mvc.Models.Navision;

public partial class LeaveStatisticsFactbox
{
    public string? Key { get; set; }
    public decimal Leave_Entitlment { get; set; }
    public bool Leave_EntitlmentSpecified { get; set; }
    public decimal LeaveEarnedToDate { get; set; }
    public bool LeaveEarnedToDateSpecified { get; set; }
    public decimal Recalled_Days { get; set; }
    public bool Recalled_DaysSpecified { get; set; }
    public decimal Days_Absent { get; set; }
    public bool Days_AbsentSpecified { get; set; }
    public decimal Balance_brought_forward { get; set; }
    public bool Balance_brought_forwardSpecified { get; set; }
    public decimal Total_Leave_Days_Taken { get; set; }
    public bool Total_Leave_Days_TakenSpecified { get; set; }
    public decimal Leave_Balance { get; set; }
    public bool Leave_BalanceSpecified { get; set; }
}
