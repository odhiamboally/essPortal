namespace ESSPortal.Application.Utilities;
public static class CacheKeys
{
    // Dashboard
    public static string Dashboard(string employeeNo) => $"dashboard_{employeeNo}";

    // Leave
    public static string LeaveTypes() => "leave_types";
    public static string LeaveHistory(string employeeNo) => $"leave_history_{employeeNo}";
    public static string AnnualLeaveSummary(string employeeNo) => $"annual_leave_summary_{employeeNo}";
    public static string LeaveSummary(string employeeNo) => $"leave_summary_{employeeNo}";

    // User pattern for bulk removal
    public static string UserPattern(string employeeNo) => $"*_{employeeNo}";
}
