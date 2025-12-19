using System;
using System.Collections.Generic;
using System.Text;

namespace ESSPortal.Application.Dtos.Leave;

public record CreateLeaveRelieverRequest
{
    public string ApplicationNo { get; init; } = string.Empty;
    public string? StaffName { get; init; }
    public string? LeaveCode { get; init; }
}
