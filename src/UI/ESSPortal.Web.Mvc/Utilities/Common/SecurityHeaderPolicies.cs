namespace ESSPortal.Web.Mvc.Utilities.Common;

public static class SecurityHeaderPolicies
{
    public static HeaderPolicyCollection BuildDevelopment() =>
        new HeaderPolicyCollection()
            .AddFrameOptionsDeny()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .AddContentSecurityPolicy(csp =>
            {
                csp.AddDefaultSrc().Self();
                csp.AddScriptSrc()
                    .Self()
                    .WithNonce()
                    .UnsafeEval()                           // Hot reload, browser tooling
                    .From("ws://localhost:*")
                    .From("wss://localhost:*")
                    .From("http://localhost:*")
                    .From("https://localhost:*");
                csp.AddConnectSrc()
                    .Self()
                    .From("ws://localhost:*")
                    .From("wss://localhost:*")
                    .From("http://localhost:*")
                    .From("https://localhost:*");
                csp.AddImgSrc().Self().Data().Blob().OverHttps().OverHttps();
                csp.AddStyleSrc()
                    .Self()
                    .UnsafeInline()                         // MudBlazor dynamic styles
                    .From("https://fonts.googleapis.com")
                    .From("https://cdnjs.cloudflare.com");
                csp.AddFontSrc()
                    .Self()
                    .From("https://fonts.gstatic.com")
                    .From("https://cdnjs.cloudflare.com");
                csp.AddObjectSrc().None();
                csp.AddBaseUri().Self();
                csp.AddFormAction().Self();
            });

    public static HeaderPolicyCollection BuildProduction(string apiBaseUrl) =>
        new HeaderPolicyCollection()
            .AddFrameOptionsDeny()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 63072000) // 2 years
            .AddPermissionsPolicy(permissions =>
            {
                permissions.AddAccelerometer().None();
                permissions.AddCamera().None();
                permissions.AddGeolocation().None();
                permissions.AddGyroscope().None();
                permissions.AddMagnetometer().None();
                permissions.AddMicrophone().None();
                permissions.AddPayment().None();
                permissions.AddUsb().None();
            })
            .AddContentSecurityPolicy(csp =>
            {
                csp.AddDefaultSrc().Self();
                csp.AddScriptSrc()
                    .Self()
                    .WithNonce();                           // Strict — no unsafe-inline, no eval
                csp.AddConnectSrc()
                    .Self()
                    .From(apiBaseUrl);                      // Config-driven, not Request.Host
                csp.AddImgSrc().Self().OverHttps().Data().Blob();
                csp.AddStyleSrc()
                    .Self()
                    .UnsafeInline()                         // MudBlazor requires this
                    .From("https://fonts.googleapis.com")
                    .From("https://cdnjs.cloudflare.com");
                csp.AddFontSrc()
                    .Self()
                    .From("https://fonts.gstatic.com")
                    .From("https://cdnjs.cloudflare.com");
                csp.AddObjectSrc().None();
                csp.AddBaseUri().Self();
                csp.AddFormAction().Self();
                csp.AddFrameAncestors().None();
            });
}
