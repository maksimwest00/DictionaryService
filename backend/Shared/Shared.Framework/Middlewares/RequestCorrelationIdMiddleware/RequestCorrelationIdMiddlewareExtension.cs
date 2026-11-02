using Microsoft.AspNetCore.Builder;

namespace Shared.Framework.Middlewares.RequestCorrelationIdMiddleware;

public static class RequestCorrelationIdMiddlewareExtension
{
    public static IApplicationBuilder UseRequestCorrelationIdMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestCorrelationIdMiddleware>();
    }
}