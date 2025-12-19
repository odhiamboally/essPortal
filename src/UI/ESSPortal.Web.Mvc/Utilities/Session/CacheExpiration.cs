namespace ESSPortal.Web.Mvc.Utilities.Session;

public static class CacheExpiration
{
    public static readonly TimeSpan SessionId = TimeSpan.FromHours(8);
    public static readonly TimeSpan UserInfo = TimeSpan.FromHours(1);
    public static readonly TimeSpan CurrentUser = TimeSpan.FromHours(1);
    public static readonly TimeSpan Dashboard = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan LeaveTypes = TimeSpan.FromMinutes(60);
    public static readonly TimeSpan LeaveHistory = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan LeaveSummary = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan AnnualLeaveSummary = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan LeaveApplicationCards = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan LeaveApplicationLists = TimeSpan.FromMinutes(30);
}