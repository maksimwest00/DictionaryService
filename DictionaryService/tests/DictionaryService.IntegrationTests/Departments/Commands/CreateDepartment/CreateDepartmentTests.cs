using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Locations;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;
using Name = DictionaryService.Domain.Locations.Name;

namespace DictionaryService.IntegrationTests.Departments.Commands.CreateDepartment;

public class CreateDepartmentTests : DictionaryBaseTests
{
    public CreateDepartmentTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ppp",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);


        var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid>>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Department department = await db.Departments
                .FirstAsync(d => d.Id == (envelope.Result), cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotEqual(Guid.Empty, envelope!.Result);
        });
    }

    [Fact]
    public async Task CreateDepartment_With_Empty_Name_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "",
            "ppp",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_Short_Name_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "ab",
            "ppp",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_Too_Long_Name_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            new string('a', 151),
            "ppp",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_Empty_Identifier_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_Short_Identifier_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ab",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_Too_Long_Identifier_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            new string('a', 151),
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_NonLatin_Identifier_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ппп",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_Identifier_With_Digits_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ppp123",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_Empty_LocationIds_Should_Fail()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ppp",
            null,
            []);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_Duplicate_LocationIds_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ppp",
            null,
            [locationId, locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_NonExistent_LocationIds_Should_Fail()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ppp",
            null,
            [Guid.NewGuid()]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_With_NonExistent_ParentId_Should_Fail()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ppp",
            Guid.NewGuid(),
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
}