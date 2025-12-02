namespace EssPortal.Web.Mvc.ViewModels.Common;

public class ErrorViewModel
{
    public string RequestId { get; set; } = string.Empty;
    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? ReferenceCode { get; set; } 
    public string? Details { get; set; }
    public int? StatusCode { get; set; }

}
