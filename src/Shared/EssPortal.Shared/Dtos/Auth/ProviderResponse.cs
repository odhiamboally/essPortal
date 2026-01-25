
namespace EssPortal.Shared.Dtos.Auth;

public record ProviderResponse(
    List<TwoFactorProvider>? Providers,
    string PreferredProvider = ""
    );
    
