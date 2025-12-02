namespace ESSPortal.Domain.NavEntities;

public class AttachedDocumentsDetails
{
    public int Table_ID { get; set; }
    public string? No { get; set; }
    public string? Document_Type { get; set; }
    public int Line_No { get; set; }
    public int ID { get; set; }
    public string? Name { get; set; }
    public string? File_Extension { get; set; }
    public string? File_Type { get; set; }
    public string? User { get; set; }
    public DateTimeOffset? Attached_Date { get; set; }
    public bool? Document_Flow_Purchase { get; set; }
    public bool? Document_Flow_Sales { get; set; }
}
