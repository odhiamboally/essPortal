using EssPortal.Web.Mvc.Middleware;


namespace ESSPortal.Web.Mvc.Extensions;



public static class MiddlwareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
    {
        return builder
            .UseMiddleware<TokenRefreshMiddleware>();
    }
}