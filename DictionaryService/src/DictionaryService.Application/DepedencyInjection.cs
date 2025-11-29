using DictionaryService.Application.Locations;
using Microsoft.Extensions.DependencyInjection;

namespace DictionaryService.Application;

public static class DepedencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILocationSerivce, LocationSerivce>();
        return services;
    }
}