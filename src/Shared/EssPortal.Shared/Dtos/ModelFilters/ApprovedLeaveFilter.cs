using EssPortal.Shared.Dtos.ModelFilters;

namespace ESSPortal.Shared.Dtos.ModelFilters;
public record ApprovedLeaveFilter : BaseFilter
{
    public string Employee_No { get; init; } = string.Empty;
}
