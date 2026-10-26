using Serilog;
using Shared.Framework.Middlewares;

namespace DictionaryService.Web;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseCors(builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
        app.UseExceptionMiddleware();
        app.UseRequestCorrelationIdMiddleware();

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "DictionaryService");
            });
        }

        app.MapControllers();

        return app;
    }
}