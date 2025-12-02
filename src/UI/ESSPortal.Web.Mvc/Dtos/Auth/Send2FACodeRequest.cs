using Microsoft.AspNetCore.Mvc.Rendering;

namespace EssPortal.Web.Mvc.Dtos.Auth;



public record Send2FACodeRequest
{
    public string UserId { get; init; } = string.Empty;
    public string? SelectedProvider { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? ReturnUrl { get; init; }
    public bool RememberMe { get; init; }
    public bool RememberBrowser { get; init; }
    public bool ForceResend { get; init; } = false;
    public List<SelectListItem>? Providers { get; init; } = [];
}
