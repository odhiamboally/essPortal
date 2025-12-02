using ESSPortal.Api.Middleware;

namespace ESSPortal.Api.Extensions;

public static class MiddlwareExtensions
{
    public static IApplicationBuilder UsePreAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PayloadEncryptionMiddleware>();
    }

    public static IApplicationBuilder UsePostAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}
