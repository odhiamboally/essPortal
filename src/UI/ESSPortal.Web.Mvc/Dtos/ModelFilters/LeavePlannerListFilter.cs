namespace EssPortal.Web.Mvc.Dtos.ModelFilters;

internal record LeavePlannerListFilter //: BaseFilter
{
    public string? No { get; init; }
    public string? EmployeeNo { get; init; }
    public string? EmployeeName { get; init; }
    public DateTime? Date { get; init; }

  public Dictionary<string, string?> CustomQueryParameters()
  {
     var parameters = new Dictionary<string, string?>();

     void AddIf(string key, object? value)
     {
        if (value != null)
           parameters[key] = value?.ToString();
     }

     AddIf(nameof(No), No);
     AddIf(nameof(EmployeeNo), EmployeeNo);
     AddIf(nameof(EmployeeName), EmployeeName);
     AddIf(nameof(Date), Date?.ToString("yyyy-MM-dd"));

     return parameters;
  }
}
