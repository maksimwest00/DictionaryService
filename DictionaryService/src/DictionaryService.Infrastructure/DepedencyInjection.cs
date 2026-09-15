using DictionaryService.Application.Database;
using DictionaryService.Application.Departments;
using DictionaryService.Application.Locations;
using DictionaryService.Application.Positions;
using DictionaryService.Infrastructure.Database;
using DictionaryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DictionaryService.Infrastructure;

public static class DepedencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DictionaryServiceDbContext>(_ => new DictionaryServiceDbContext(
            configuration.GetConnectionString("DictionaryServiceDb")!));

        services.AddScoped<IReadDbContext, DictionaryServiceDbContext>(_ => new DictionaryServiceDbContext(
            configuration.GetConnectionString("DictionaryServiceDb")!));

        services.AddSingleton(sp =>
        {
            string connectionString = configuration.GetConnectionString("DictionaryServiceDb")!;
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

            dataSourceBuilder.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());

            return dataSourceBuilder.Build();
        });

        services.AddSingleton<IReadDbConnectionFactory, NpgsqlReadDbConnectionFactory>();
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        return services;
    }
}