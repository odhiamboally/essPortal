namespace ESSPortal.Application.Utilities;

public static class CacheKeys
{
    public static string UserInfo(string employeeNo) => $"UserInfo_{employeeNo}";
    public static string CurrentUser() => $"CurrentUser";
    public static string Dashboard(string employeeNo) => $"Dashboard_{employeeNo}";
    public static string LeaveTypes() => "LeaveTypes";
    public static string LeaveHistory(string employeeNo) => $"LeaveHistory_{employeeNo}";
    public static string LeaveSummary(string employeeNo) => $"LeaveSummary_{employeeNo}";
    public static string AnnualLeaveSummary(string employeeNo) => $"AnnualLeaveSummary_{employeeNo}";
    public static string LeaveFormData(string employeeNo) => $"LeaveFormData_{employeeNo}";
    public static string LeaveApplicationCards(string employeeNo) => $"LeaveApplicationCards_{employeeNo}";
    public static string LeaveApplicationLists(string employeeNo) => $"LeaveApplicationLists_{employeeNo}";
    public static IEnumerable<string> AllKeysForEmployee(string employeeNo)
    {
        return new[]
        {
            UserInfo(employeeNo),
            CurrentUser(),
            Dashboard(employeeNo),
            LeaveHistory(employeeNo),
            LeaveSummary(employeeNo),
            AnnualLeaveSummary(employeeNo),
            LeaveFormData(employeeNo),
            LeaveApplicationCards(employeeNo),
            LeaveApplicationLists(employeeNo)
        };
    }


    // User pattern for bulk removal
    public static string UserPattern(string employeeNo) => $"*_{employeeNo}";
}
