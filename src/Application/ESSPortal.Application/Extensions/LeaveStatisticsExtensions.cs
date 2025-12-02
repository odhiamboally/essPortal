using ESSPortal.Application.Dtos.Leave;
using ESSPortal.Application.Mappings;

namespace ESSPortal.Application.Extensions;

public static class LeaveStatisticsExtensions
{
    /// <summary>
    /// Get annual leave statistics from comprehensive response
    /// </summary>
    public static AnnualLeaveSummaryResponse ToAnnualLeaveSummary(this LeaveSummaryResponse comprehensive)
    {
        return BCEntityMappingExtensions.FromComprehensive(comprehensive);
    }

    /// <summary>
    /// Get specific leave type balance
    /// </summary>
    public static decimal GetCurrentBalance(this LeaveSummaryResponse comprehensive, string leaveCode)
    {
        return comprehensive.GetLeaveType(leaveCode).CurrentBalance;
    }

    /// <summary>
    /// Check if employee has sufficient balance for a leave type
    /// </summary>
    public static bool HasSufficientBalance(this LeaveSummaryResponse comprehensive, string leaveCode, decimal daysRequested)
    {
        return comprehensive.GetLeaveType(leaveCode).CurrentBalance >= daysRequested;
    }

    /// <summary>
    /// Get leave types with available balance
    /// </summary>
    public static List<LeaveTypeStatistics> GetAvailableLeaveTypes(this LeaveSummaryResponse comprehensive)
    {
        return comprehensive.GetAllLeaveTypes()
            .Where(lt => lt.CurrentBalance > 0)
            .ToList();
    }

    /// <summary>
    /// Get leave types that are running low (less than 25% remaining)
    /// </summary>
    public static List<LeaveTypeStatistics> GetLowBalanceLeaveTypes(this LeaveSummaryResponse comprehensive)
    {
        return comprehensive.GetAllLeaveTypes()
            .Where(lt => lt.YearlyEntitlement > 0 && (lt.CurrentBalance / lt.YearlyEntitlement) < 0.25m)
            .ToList();
    }
}