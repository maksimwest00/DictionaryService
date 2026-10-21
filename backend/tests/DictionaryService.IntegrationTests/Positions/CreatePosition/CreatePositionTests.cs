using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Positions;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Positions;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.IntegrationTests.Positions.CreatePosition;

public class CreatePositionTests : DictionaryBaseTests
{
    public CreatePositionTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreatePosition_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocationDb("Локация 1");
        Guid departmentId = await CreateDepartmentDb(
            locationId,
            "Подразделение",
            "aaa");
        CancellationToken cancellationToken = CancellationToken.None;

        string uniquePositionName = $"Позиция_{Guid.NewGuid()}";

        // Act
        using var client = this.CreateClient();

        Guid[] departmentIds = [departmentId];
        CreatePositionRequest request = new(
            uniquePositionName,
            "Описание",
            departmentIds);

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request,
            cancellationToken: cancellationToken);

        // Assert

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid>>(cancellationToken);


        await ExecuteInDb(async db =>
        {
            Position position = await db.Positions
                .Include(p => p.DepartmentPositions)
                .FirstAsync(d => d.Id == envelope.Result, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotEqual(Guid.Empty, envelope.Result);
            Assert.Equal(uniquePositionName, position.Name.Value);
            Assert.Equal("Описание", position.Description.Value);
            Assert.Single(position.DepartmentPositions);
            Assert.Equal(departmentId, position.DepartmentPositions.First().DepartmentId);
            Assert.NotEqual(Guid.Empty, position.DepartmentPositions.First().Id);
            Assert.True(position.IsActive);
            Assert.True(position.CreatedAt > DateTime.MinValue);
            Assert.True(position.UpdatedAt > DateTime.MinValue);
        });
    }

    [Fact]
    public async Task CreatePosition_With_Duplicate_Name_Should_Return_Conflict()
    {
        // Arrange
        Guid locationId = await CreateLocationDb("Локация 1");
        Guid departmentId = await CreateDepartmentDb(
            locationId,
            "Подразделение",
            "aaa");
        CancellationToken cancellationToken = CancellationToken.None;

        string positionName = $"Позиция_{Guid.NewGuid()}";

        // Create first position
        using var client = this.CreateClient();

        CreatePositionRequest request1 = new(
            positionName,
            "Описание",
            [departmentId]);

        await client.PostAsJsonAsync(
            "/api/positions",
            request1,
            cancellationToken: cancellationToken);

        // Act - try to create with same name
        CreatePositionRequest request2 = new(
            positionName,
            "Описание2",
            [departmentId]);

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request2,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreatePosition_With_Non_Existent_Department_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Guid nonExistentDepartmentId = Guid.NewGuid();

        // Act
        using var client = this.CreateClient();

        CreatePositionRequest request = new(
            "Позиция",
            "Описание",
            [nonExistentDepartmentId]);

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePosition_With_Empty_DepartmentIds_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreatePositionRequest request = new(
            "Позиция",
            "Описание",
            Array.Empty<Guid>());

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePosition_With_Empty_Name_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreatePositionRequest request = new(
            "",
            "Описание",
            [Guid.NewGuid()]);

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePosition_With_Name_Too_Short_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreatePositionRequest request = new(
            "ab",
            "Описание",
            [Guid.NewGuid()]);

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePosition_With_Name_Too_Long_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        string longName = new('a', 101);

        // Act
        using var client = this.CreateClient();

        CreatePositionRequest request = new(
            longName,
            "Описание",
            [Guid.NewGuid()]);

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePosition_With_Description_Too_Long_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        string longDescription = new('a', 1001);

        // Act
        using var client = this.CreateClient();

        CreatePositionRequest request = new(
            "Позиция",
            longDescription,
            [Guid.NewGuid()]);

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePosition_With_Duplicate_DepartmentIds_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Guid departmentId = Guid.NewGuid();

        // Act
        using var client = this.CreateClient();

        CreatePositionRequest request = new(
            "Позиция",
            "Описание",
            [departmentId, departmentId]);

        using var response = await client.PostAsJsonAsync(
            "/api/positions",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}