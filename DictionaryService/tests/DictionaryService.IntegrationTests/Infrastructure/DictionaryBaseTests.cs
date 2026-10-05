using System.Net.Http.Json;
using DictionaryService.Application.Departments.Commands.CreateDepartment;
using DictionaryService.Contracts.Departments;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Locations;
using DictionaryService.Domain.Positions;
using DictionaryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Name = DictionaryService.Domain.Locations.Name;

namespace DictionaryService.IntegrationTests.Infrastructure;

public class DictionaryBaseTests : IClassFixture<DictionaryTestWebFactory>, IAsyncLifetime
{
    private readonly DictionaryTestWebFactory _factory;
    private readonly Func<Task> _resetDatabase;

    public DictionaryBaseTests(DictionaryTestWebFactory factory)
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
        await action(dbContext);
    }

    protected async Task<Guid> CreateLocationDb(string name = "Локация")
    {
        return await ExecuteInDb(async db =>
        {
            Location location = new(
                Name.Create(name).Value,
                Address.Create(
                    "Город",
                    "Улица",
                    "Дом",
                    "Номер квартиры").Value,
                "TimeZone");

            db.Locations.Add(location);
            await db.SaveChangesAsync();

            return location.Id;
        });
    }

    protected async Task<Guid> CreateDepartmentDb(Guid locationId, string name = "Подразделение", string identifier = "ppp")
    {
        return await ExecuteInDb(async db =>
        {
            var departmentResult = Department.CreateParent(
                DictionaryService.Domain.Departments.Name.Create(name).Value,
                DictionaryService.Domain.Departments.Identifier.Create(identifier).Value,
                [locationId]);

            if (departmentResult.IsFailure)
                throw new Exception(string.Join(", ", departmentResult.Error.Messages));

            db.Departments.Add(departmentResult.Value);
            await db.SaveChangesAsync();

            return departmentResult.Value.Id;
        });
    }

    protected async Task CreateDepartmentApi(Guid locationId, string name, string identifier)
    {
        using var client = CreateClient();

        CreateDepartmentRequest request = new(
            name,
            identifier,
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: System.Threading.CancellationToken.None);

        response.EnsureSuccessStatusCode();
    }

    protected async Task<Guid> CreatePosition(string name = "Должность", string description = "Описание")
    {
        return await ExecuteInDb(async db =>
        {
            Position position = Position.Create(
                DictionaryService.Domain.Positions.Name.Create(name).Value,
                DictionaryService.Domain.Positions.Description.Create(description).Value,
                []);

            db.Positions.Add(position);
            await db.SaveChangesAsync();

            return position.Id;
        });
    }
}