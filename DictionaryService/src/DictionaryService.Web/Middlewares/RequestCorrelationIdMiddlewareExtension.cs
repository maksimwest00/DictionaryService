namespace DictionaryService.Web.Middlewares;

public static class RequestCorrelationIdMiddlewareExtension
{
    public static IApplicationBuilder UseRequestCorrelationIdMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestCorrelationIdMiddleware>();
    }
}