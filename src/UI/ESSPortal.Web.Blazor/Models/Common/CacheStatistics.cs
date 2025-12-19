namespace ESSPortal.Web.Blazor.Models.Common;

public class CacheStatistics
{
    public string EmployeeNo { get; set; } = string.Empty;
    public bool HasDashboardCache { get; set; }
    public bool HasLeaveHistoryCache { get; set; }
    public bool HasLeaveTypesCache { get; set; }
    public bool HasLeaveSummaryCache { get; set; }
    public DateTime Timestamp { get; set; }

    public int CachedItemsCount =>
        (HasDashboardCache ? 1 : 0) +
        (HasLeaveHistoryCache ? 1 : 0) +
        (HasLeaveTypesCache ? 1 : 0);

    
}
