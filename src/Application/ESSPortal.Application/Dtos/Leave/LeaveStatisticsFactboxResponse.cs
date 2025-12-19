using System;
using System.Collections.Generic;
using System.Text;

namespace ESSPortal.Application.Dtos.Leave;

public record LeaveStatisticsFactboxResponse
{
    public string? Key { get; init; }

    public decimal LeaveEntitlement { get; init; }
    public bool LeaveEntitlementSpecified { get; init; }

    public decimal LeaveEarnedToDate { get; init; }
    public bool LeaveEarnedToDateSpecified { get; init; }

    public decimal RecalledDays { get; init; }
    public bool RecalledDaysSpecified { get; init; }

    public decimal DaysAbsent { get; init; }
    public bool DaysAbsentSpecified { get; init; }

    public decimal BalanceBroughtForward { get; init; }
    public bool BalanceBroughtForwardSpecified { get; init; }

    public decimal TotalLeaveDaysTaken { get; init; }
    public bool TotalLeaveDaysTakenSpecified { get; init; }

    public decimal LeaveBalance { get; init; }
    public bool LeaveBalanceSpecified { get; init; }
}
