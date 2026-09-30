using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments.GetDepartmentsByFilters;
using DictionaryService.Contracts.Shared;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Locations;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;
using Name = DictionaryService.Domain.Locations.Name;

namespace DictionaryService.IntegrationTests.Departments;

public class GetDepartmentsByFiltersTests : DictionaryBaseTests
{
    public GetDepartmentsByFiltersTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartmentsByFilters_With_Default_Params_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        await CreateDepartment(locationId, "Подразделение1", "aaa");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
    }

    [Fact]
    public async Task GetDepartmentsByFilters_With_Search_Should_Filter_Results()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        await CreateDepartment(locationId, "Подразделение1", "ppp");
        await CreateDepartment(locationId, "Подразделение2", "rrr");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments?search=Подразделение1",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Contains("Подразделение1", result.Data.First().Name);
    }

    [Fact]
    public async Task GetDepartmentsByFilters_With_Pagination_Should_Return_Correct_Page()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        char c = 'a';
        for (int i = 0; i < 5; i++)
        {
            await CreateDepartment(locationId, $"Подразделение{i + 1}", $"{c}aa");
            c++;
        }
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments?page=1&pageSize=2",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetDepartmentsByFilters_With_Invalid_Page_Should_Return_Empty_Result()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        await CreateDepartment(locationId, "Подразделение1", "ppp");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments?page=999&pageSize=10",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetDepartmentsByFilters_With_Invalid_PageSize_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments?pageSize=0",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentsByFilters_With_Negative_Page_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments?page=-1",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentsByFilters_With_SortBy_Should_Order_Results()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        await CreateDepartment(locationId, "B_Department", "aaa");
        await CreateDepartment(locationId, "A_Department", "bbb");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments?sortBy=name&sortDir=asc",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.True(result.Data.Count >= 2);
        Assert.Equal("A_Department", result.Data.First().Name);
    }

    private async Task<Guid> CreateLocation() =>
        await ExecuteInDb(async db =>
        {
            Location location = new(
                Name.Create("Локация").Value,
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

    private async Task<Guid> CreateDepartment(
        Guid locationId,
        string name = "Подразделение",
        string identifier = "") =>
        await ExecuteInDb(async db =>
        {
            var nameValue = DictionaryService.Domain.Departments.Name.Create(name).Value;
            var identifierValue = DictionaryService.Domain.Departments.Identifier.Create(identifier).Value;

            var departmentResult = Department.CreateParent(
                nameValue,
                identifierValue,
                [locationId]);

            if (departmentResult.IsFailure)
                throw new Exception(string.Join(", ", departmentResult.Error.Messages));

            db.Departments.Add(departmentResult.Value);
            await db.SaveChangesAsync();

            return departmentResult.Value.Id;
        });
}
