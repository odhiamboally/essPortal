namespace ESSPortal.Web.Mvc.Models.Profile;

public class ProfileActivityItem
{
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = "📝";
}
