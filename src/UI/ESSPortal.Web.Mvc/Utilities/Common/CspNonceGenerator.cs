using System.Security.Cryptography;

namespace ESSPortal.Web.Mvc.Utilities.Common;

public static class CspNonceGenerator
{
    private const string NonceKey = "CSP_NONCE";

    public static string Generate(HttpContext context)
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items[NonceKey] = nonce;
        return nonce;
    }

    public static string Get(HttpContext context)
    {
        return context.Items.TryGetValue(NonceKey, out var value)
            ? value?.ToString() ?? ""
            : "";
    }
}
