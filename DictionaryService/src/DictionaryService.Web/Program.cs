using System.Globalization;
using DictionaryService.Web;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Services.AddProgramDependencies(builder.Configuration);

    WebApplication app = builder.Build();

    app.Configure();

    app.Run();
}
catch (Exception ex)
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
