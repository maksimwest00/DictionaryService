using System.Data.Common;
using DictionaryService.Application.Database;
using DictionaryService.Infrastructure;
using DictionaryService.Infrastructure.Database;
using DictionaryService.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DictionaryService.IntegrationTests.Infrastructure;

public class DictionaryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("dictionary_service_db")
        .WithUsername("postgres")
        .WithPassword("password")
        .Build();

    private Respawner _respawner;
    private DbConnection _dbConnection;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await using var scope = this.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DictionaryServiceDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        await InitializeRespawner();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DictionaryServiceDbContext>();
            services.RemoveAll<IReadDbContext>();

            services.AddScoped<DictionaryServiceDbContext>(_ =>
                new DictionaryServiceDbContext(_dbContainer.GetConnectionString()));

            services.AddScoped<IReadDbContext, DictionaryServiceDbContext>(_ =>
                new DictionaryServiceDbContext(_dbContainer.GetConnectionString()));

            services.RemoveAll<NpgsqlDataSource>();
            services.AddSingleton(sp =>
            {
                string connectionString = _dbContainer.GetConnectionString();
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

                dataSourceBuilder.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());

                return dataSourceBuilder.Build();
            });

            services.RemoveAll<IReadDbConnectionFactory>();
            services.AddSingleton<IReadDbConnectionFactory, NpgsqlReadDbConnectionFactory>();
        });
    }
}