using System;
using System.Collections.Generic;
using System.Text;

namespace ESSPortal.Application.Dtos.Leave;

public record CreateLeaveApplicationListRequest
{
    public string EmployeeNo { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string LeavePeriod { get; init; } = string.Empty;
}
