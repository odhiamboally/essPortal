namespace EssPortal.Shared.Dtos.ModelFilters;

public record LeaveRelieverFilter : BaseFilter
{
    public string? StaffNo { get; init; }
    public string? StaffName { get; init; }
    public string? LeaveCode { get; init; }


}
