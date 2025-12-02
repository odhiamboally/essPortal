namespace EssPortal.Web.Mvc.Dtos.ModelFilters;

public record LeaveStatisticsFactboxFilter //: BaseFilter
{
    public string? EmployeeNumber { get; init; }
    public string? LeaveEntitlment { get; init; }
    public string? LeaveEarnedToDate { get; init; }
    public string? RecalledDays { get; init; }
    public string? DaysAbsent { get; init; }
    public string? BalanceBroughtForward { get; init; }
    public string? TotalLeaveDaysTaken { get; init; }
    public string? LeaveBalance { get; init; }

  public Dictionary<string, string?> CustomQueryParameters()
  {
     var parameters = new Dictionary<string, string?>();

     void AddIf(string key, string? value)
     {
        if (!string.IsNullOrWhiteSpace(value))
           parameters[key] = value;
     }

     AddIf(nameof(EmployeeNumber), EmployeeNumber);
     AddIf(nameof(LeaveEntitlment), LeaveEntitlment);
     AddIf(nameof(LeaveEarnedToDate), LeaveEarnedToDate);
     AddIf(nameof(RecalledDays), RecalledDays);
     AddIf(nameof(DaysAbsent), DaysAbsent);
     AddIf(nameof(BalanceBroughtForward), BalanceBroughtForward);
     AddIf(nameof(TotalLeaveDaysTaken), TotalLeaveDaysTaken);
     AddIf(nameof(LeaveBalance), LeaveBalance);

     return parameters;
  }
}
