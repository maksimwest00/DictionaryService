using DictionaryService.Application;
using DictionaryService.Infrastructure;

namespace DictionaryService.Web;

public static class DepedencyInjection
{
    public static IServiceCollection AddProgramDependincies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddWebDependencies();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}