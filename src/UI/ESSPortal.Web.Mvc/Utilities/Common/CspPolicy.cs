namespace ESSPortal.Web.Mvc.Utilities.Common;

public static class CspPolicy
{
    public static string Build(string selfOrigin, string nonce, bool isDev)
    {
        if (isDev)
        {
            // Dev still needs websocket + tooling flexibility
            return
                "default-src 'self'; " +
                $"script-src 'self' 'nonce-{nonce}' ws://localhost:* wss://localhost:* http://localhost:* https://localhost:*; " +
                $"connect-src 'self' ws://localhost:* wss://localhost:* http://localhost:* https://localhost:*; " +
                "img-src 'self' data: blob: https: http:; " +
                "style-src 'self' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
                "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                "object-src 'none'; base-uri 'self'; form-action 'self';";
        }

        return
            "default-src 'self'; " +
            $"script-src 'self' 'nonce-{nonce}'; " +
            $"connect-src 'self' {selfOrigin}; " +
            "img-src 'self' https: data: blob:; " +
            "style-src 'self' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
            "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
            "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none';";
    }
}
