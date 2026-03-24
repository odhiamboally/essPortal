using ESSPortal.Web.Mvc.Utilities.Common;

namespace ESSPortal.Web.Mvc.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env, IConfiguration config)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var nonce = CspNonceGenerator.Generate(context);

        var selfOrigin = env.IsDevelopment()
            ? $"{context.Request.Scheme}://{context.Request.Host}"
            : config["AppSettings:VirtualPath"]?.TrimEnd('/') ?? string.Empty;

        var csp = CspPolicy.Build(selfOrigin, nonce, env.IsDevelopment());

        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers.XFrameOptions = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers.ContentSecurityPolicy = csp;

        if (!env.IsDevelopment())
        {
            context.Response.Headers["Permissions-Policy"] =
                "geolocation=(), microphone=(), camera=(), payment=(), usb=(), " +
                "magnetometer=(), accelerometer=(), gyroscope=()";
        }

        await next(context);
    }
}