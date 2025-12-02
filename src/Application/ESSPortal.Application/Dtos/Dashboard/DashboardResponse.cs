using ESSPortal.Application.Dtos.Leave;

namespace ESSPortal.Application.Dtos.Dashboard;
public record DashboardResponse(
    string EmployeeNo,
    string EmployeeName,
    AnnualLeaveSummaryResponse? AnnualLeaveSummary,
    LeaveSummaryResponse? LeaveSummary,
    List<ApprovedLeaveResponse> ApprovedLeaves,
    List<LeaveApplicationCardResponse> LeaveApplicationCards,
    List<LeaveApplicationListResponse> LeaveApplicationLists,
    List<LeaveHistoryResponse> LeaveHistory, // Add this
    List<LeaveTypeResponse> LeaveTypes,
    List<LeaveRelieverResponse> LeaveRelievers
)
{
    
    public string CurrentLeavePeriod => DateTime.Now.Year.ToString();

    public DateTime GeneratedAt => DateTime.Now;

    public int TotalApplications => LeaveApplicationCards.Count;

    public int AvailableLeaveTypes => LeaveTypes.Count;
};

