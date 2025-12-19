namespace EssPortal.Web.Blazor.Dtos.ModelFilters;

public record LeavePlannerLinesFilter //: BaseFilter
{
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public string? LeaveType { get; init; }
    public decimal? NoOfDays { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime? ResumptionDate { get; init; }

  public Dictionary<string, string?> CustomQueryParameters()
  {
     var parameters = new Dictionary<string, string?>();

     void AddIf(string key, object? value)
     {
        if (value != null)
           parameters[key] = value?.ToString();
     }

     AddIf(nameof(EmployeeNo), EmployeeNo);
     AddIf(nameof(EmployeeName), EmployeeName);
     AddIf(nameof(LeaveType), LeaveType);
     AddIf(nameof(NoOfDays), NoOfDays);
     AddIf(nameof(StartDate), StartDate?.ToString("yyyy-MM-dd"));
     AddIf(nameof(EndDate), EndDate?.ToString("yyyy-MM-dd"));
     AddIf(nameof(ResumptionDate), ResumptionDate?.ToString("yyyy-MM-dd"));

     return parameters;
  }
}
