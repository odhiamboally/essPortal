namespace ESSPortal.Application.Dtos.ModelFilters;
public record ApprovedLeaveFilter : BaseFilter
{
    public string Employee_No { get; init; } = string.Empty;
}
