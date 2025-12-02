namespace ESSPortal.Application.Dtos.ModelFilters;
public record LeaveAttachementFilter : BaseFilter
{
    public int Table_ID { get; init; } 
    public string No { get; init; } = string.Empty;
}
