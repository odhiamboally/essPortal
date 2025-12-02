namespace EssPortal.Web.Mvc.Models.Navision;

public class Leave
{
    public LeaveApplicationCard? LeaveApplicationCard { get; set; }
    public LeaveReliever? LeaveReliever { get; set; }
    public List<LeaveStatisticsFactbox>? LeaveStatisticsFactboxes { get; set; }

    public int Ischecked { get; set; }
    public string? ApproveType { get; set; }
}
