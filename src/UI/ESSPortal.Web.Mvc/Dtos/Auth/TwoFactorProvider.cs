namespace EssPortal.Web.Mvc.Dtos.Auth;

public record TwoFactorProvider
{
    public string Value { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public bool Selected { get; set; }
    public bool IsDefault { get; init; }
    public string? MaskedDestination { get; init; }
}
