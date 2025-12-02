namespace EssPortal.Web.Mvc.Dtos.ModelFilters;

public record LeaveRelieverFilter
{
    public string? StaffNo { get; init; }
    public string? StaffName { get; init; }
    public string? LeaveCode { get; init; }


    public Dictionary<string, string?> CustomQueryParameters()
    {
       var parameters = new Dictionary<string, string?>();

       void AddIf(string key, string? value)
       {
          if (!string.IsNullOrWhiteSpace(value))
             parameters[key] = value;
       }

       // Explicit mapping for all fields
       AddIf(nameof(StaffNo), StaffNo);
       AddIf(nameof(StaffName), StaffName);
       AddIf(nameof(LeaveCode), LeaveCode);

       return parameters;
    }
}
