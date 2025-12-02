namespace EssPortal.Web.Mvc.Dtos.ModelFilters;

internal record LeavePlannerListGeneralFilter //: BaseFilter
{
    public string? No { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public string? Date { get; init; }
    public string? Submitted { get; init; }

  public Dictionary<string, string?> CustomQueryParameters()
  {
     var parameters = new Dictionary<string, string?>();

     void AddIf(string key, string? value)
     {
        if (!string.IsNullOrWhiteSpace(value))
           parameters[key] = value;
     }

     // Explicit mapping
     AddIf(nameof(No), No);
     AddIf(nameof(EmployeeNo), EmployeeNo);
     AddIf(nameof(EmployeeName), EmployeeName);
     AddIf(nameof(Date), Date);
     AddIf(nameof(Submitted), Submitted);

     return parameters;
  }
}
