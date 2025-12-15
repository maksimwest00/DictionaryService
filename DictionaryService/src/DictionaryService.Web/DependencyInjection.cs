using DictionaryService.Application;
using DictionaryService.Infrastructure;
using Serilog;
using Serilog.Exceptions;

namespace DictionaryService.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddWebDependencies(configuration)
                       .AddApplication()
                       .AddInfrastructure(configuration);
    }

    private static IServiceCollection AddWebDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSerilogLogging(configuration);
        services.AddControllers();
        services.AddOpenApi();
        return services;
    }

    private static IServiceCollection AddSerilogLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddSerilog((sp, lc) =>
        {
            lc.ReadFrom.Configuration(configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("ServiceName", "DictionaryService");
        });
    }
}