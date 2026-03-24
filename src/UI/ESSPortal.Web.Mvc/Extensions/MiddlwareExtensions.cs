using EssPortal.Web.Mvc.Middleware;

using ESSPortal.Web.Mvc.Middleware;


namespace ESSPortal.Web.Mvc.Extensions;



public static class MiddlwareExtensions
{
    public static IApplicationBuilder UseCustomSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();

    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
    {
        return builder
            .UseMiddleware<ScreenLockMiddleware>()
            .UseMiddleware<TokenRefreshMiddleware>();
    }

    public static IApplicationBuilder UsePostAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}