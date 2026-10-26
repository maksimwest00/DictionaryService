using DictionaryService.Application;
using DictionaryService.Infrastructure;
using Shared.Framework.Logging;

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
        services.AddCors();
        
        services.AddSerilogLogging(configuration, "DictionaryService");
        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}