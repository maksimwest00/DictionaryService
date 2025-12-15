using DictionaryService.Web;
using DictionaryService.Web.Middlewares;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProgramDependincies(builder.Configuration);

WebApplication app = builder.Build();

app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
<<<<<<< Updated upstream
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "DictionaryService");
    });
=======
    Log.Information("Starting web application");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    string environment = builder.Environment.EnvironmentName;

    builder.Configuration.AddJsonFile(
        $"appsettings.{environment}.json",
        true,
        true);

    builder.Services.AddProgramDependencies(builder.Configuration);

    WebApplication app = builder.Build();

    app.Configure();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
>>>>>>> Stashed changes
}

app.MapControllers();

app.Run();