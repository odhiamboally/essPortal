using System;
using System.Collections.Generic;
using System.Text;

namespace ESSPortal.Application.Dtos.Leave;

public record CreateLeaveTypeRequest
{
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? Days { get; init; }
    public bool? Accrue_Days { get; init; }
    public decimal? Conversion_Rate_Per_Day { get; init; }
    public bool? Unlimited_Days { get; init; }
    public string? Gender { get; init; }
    public string? Balance { get; init; }
    public decimal? Max_Carry_Forward_Days { get; init; }
    public bool? Annual_Leave { get; init; }
    public bool? Inclusive_of_Holidays { get; init; }
    public bool? Inclusive_of_Saturday { get; init; }
    public bool? Inclusive_of_Sunday { get; init; }
    public bool? Off_Holidays_Days_Leave { get; init; }
    public string? Status { get; init; }
}