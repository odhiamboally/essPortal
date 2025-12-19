namespace ESSPortal.Web.Mvc.Utilities.Session;

public static class CacheKeys
{
    public static string SessionId(string employeeNo) => $"SessionId_{employeeNo}";
    public static string UserInfo(string employeeNo) => $"UserInfo_{employeeNo}";
    public static string CurrentUser(string employeeNo) => $"CurrentUser_{employeeNo}";
    public static string Dashboard(string employeeNo) => $"Dashboard_{employeeNo}";
    public static string LeaveTypes() => "LeaveTypes";
    public static string LeaveHistory(string employeeNo) => $"LeaveHistory_{employeeNo}";
    public static string LeaveSummary(string employeeNo) => $"LeaveSummary_{employeeNo}";
    public static string AnnualLeaveSummary(string employeeNo) => $"AnnualLeaveSummary_{employeeNo}";


    // User pattern for bulk removal
    public static string UserPattern(string employeeNo) => $"*_{employeeNo}";
}