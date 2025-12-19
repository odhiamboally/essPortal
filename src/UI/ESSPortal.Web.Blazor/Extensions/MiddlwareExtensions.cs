using EssPortal.Web.Blazor.Middleware;

using ESSPortal.Web.Blazor.Middleware;


namespace ESSPortal.Web.Blazor.Extensions;



public static class MiddlwareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
    {
        return builder
            .UseMiddleware<ScreenLockMiddleware>()
            .UseMiddleware<TokenRefreshMiddleware>();
    }
}