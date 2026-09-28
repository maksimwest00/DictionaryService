using DictionaryService.Application.Departments.Commands.CreateDepartment;
using DictionaryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DictionaryService.IntegrationTests.Infrastructure;

public class DictionaryBaseTests : IClassFixture<DictionaryTestWebFactory>, IAsyncLifetime
{
    private readonly DictionaryTestWebFactory _factory;
    private readonly Func<Task> _resetDatabase;

    protected DictionaryBaseTests(DictionaryTestWebFactory factory)
    {
        _factory = factory;
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    private IServiceProvider Services { get; set; }

    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    protected async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        CreateDepartmentHandler sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }

    protected async Task<T> ExecuteInDb<T>(Func<DictionaryServiceDbContext, Task<T>> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        DictionaryServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<DictionaryServiceDbContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DictionaryServiceDbContext, Task> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        DictionaryServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<DictionaryServiceDbContext>();
    }
}