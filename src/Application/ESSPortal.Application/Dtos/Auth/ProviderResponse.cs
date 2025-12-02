namespace ESSPortal.Application.Dtos.Auth;
public record ProviderResponse(
    List<TwoFactorProvider>? Providers, 
    string PreferredProvider);
