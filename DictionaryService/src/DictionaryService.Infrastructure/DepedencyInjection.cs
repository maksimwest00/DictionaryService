using DictionaryService.Application.Departments;
using DictionaryService.Application.Locations;
using DictionaryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DictionaryService.Infrastructure;

public static class DepedencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DictionaryServiceDbContext>(_ => new DictionaryServiceDbContext(
            configuration.GetConnectionString("DictionaryServiceDb")!));
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        return services;
    }
}