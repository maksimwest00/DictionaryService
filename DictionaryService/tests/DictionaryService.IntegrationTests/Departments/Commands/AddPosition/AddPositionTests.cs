using System.Net;
using System.Net.Http.Json;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.IntegrationTests.Departments.Commands.AddPosition;

public class AddPositionTests : DictionaryBaseTests
{
    public AddPositionTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AddPosition_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.PostAsync(
            $"/api/departments/{departmentId}/positions/{positionId}",
            null,
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            var departmentPosition = await db.DepartmentPositions
                .FirstOrDefaultAsync(dp => dp.DepartmentId == departmentId && dp.PositionId == positionId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotNull(departmentPosition);
        });
    }

    [Fact]
    public async Task AddPosition_With_Non_Existent_Department_Should_Return_NotFound()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        Guid nonExistentDepartmentId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.PostAsync(
            $"/api/departments/{nonExistentDepartmentId}/positions/{positionId}",
            null,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddPosition_With_Non_Existent_Position_Should_Return_NotFound()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        Guid nonExistentPositionId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.PostAsync(
            $"/api/departments/{departmentId}/positions/{nonExistentPositionId}",
            null,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddPosition_When_Already_Added_Should_Return_Conflict()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        using var client = this.CreateClient();

        // First add
        await client.PostAsync(
            $"/api/departments/{departmentId}/positions/{positionId}",
            null,
            cancellationToken);

        // Act - try to add again
        using var response = await client.PostAsync(
            $"/api/departments/{departmentId}/positions/{positionId}",
            null,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddPosition_With_Invalid_Department_Guid_Should_Return_NotFound()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.PostAsync(
            $"/api/departments/{invalidGuid}/positions/{positionId}",
            null,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddPosition_With_Invalid_Position_Guid_Should_Return_NotFound()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.PostAsync(
            $"/api/departments/{departmentId}/positions/{invalidGuid}",
            null,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
