using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

public record LeaveRelieversFilter : BaseFilter
{
    public string? ApplicationNo { get; init; }
    public string? StaffNo { get; init; }
    public string? StaffName { get; init; }
    public string? LeaveCode { get; init; }


   
}
