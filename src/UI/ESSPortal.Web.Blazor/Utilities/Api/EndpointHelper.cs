namespace ESSPortal.Web.Blazor.Utilities.Api;

public static class EndpointHelper
{
    public static string ReplaceVersion(string? endpoint, string? version = "1.0")
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));

        return endpoint.Replace("{version}", version ?? "1.0", StringComparison.OrdinalIgnoreCase);
    }

    public static string ReplaceParams(string endpoint, Dictionary<string, string> replacements)
    {
        foreach (var kv in replacements)
        {
            endpoint = endpoint.Replace($"{{{kv.Key}}}", Uri.EscapeDataString(kv.Value), StringComparison.OrdinalIgnoreCase);
        }
        return endpoint;
    }
}
