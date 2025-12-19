using EssPortal.Web.Blazor.Dtos.Auth;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace EssPortal.Web.Blazor.ViewModels.Auth;

public class TwoFactorLoginViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public string? Provider { get; set; }
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool RememberDevice { get; set; } = true;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public bool RememberBrowser { get; set; } = false;
    public bool RememberMe { get; set; } = false;
    public bool RequiresCodeSending { get; set; } = false;
    public List<SelectListItem> Providers { get; set; } = [];
    public List<TwoFactorProvider> TwoFactorProviders { get; set; } = [];
} 
