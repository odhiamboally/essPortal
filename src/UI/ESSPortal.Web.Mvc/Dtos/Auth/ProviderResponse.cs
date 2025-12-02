namespace EssPortal.Web.Mvc.Dtos.Auth;

public record ProviderResponse(
    List<TwoFactorProvider>? Providers,
    //List<SelectListItem>? Providers, 
    string PreferredProvider = ""
    );
    
