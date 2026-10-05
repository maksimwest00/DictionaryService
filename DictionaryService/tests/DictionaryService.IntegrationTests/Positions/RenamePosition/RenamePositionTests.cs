using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Positions;
using DictionaryService.Domain.Positions;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.IntegrationTests.Positions.RenamePosition;

public class RenamePositionTests : DictionaryBaseTests
{
    public RenamePositionTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task RenamePosition_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        string newName = $"Новое имя_{Guid.NewGuid()}";
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        RenamePositionRequest request = new(newName);

        using var response = await client.PatchAsJsonAsync(
            $"/api/positions/{positionId}",
            request,
            cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Position position = await db.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(newName, position.Name.Value);
        });
    }

    [Fact]
    public async Task RenamePosition_With_Non_Existent_Id_Should_Return_NotFound()
    {
        // Arrange
        Guid nonExistentPositionId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        RenamePositionRequest request = new("Новое имя");

        using var response = await client.PatchAsJsonAsync(
            $"/api/positions/{nonExistentPositionId}",
            request,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RenamePosition_With_Invalid_Guid_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        RenamePositionRequest request = new("Новое имя");

        using var response = await client.PatchAsJsonAsync(
            $"/api/positions/{invalidGuid}",
            request,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RenamePosition_With_Duplicate_Name_Should_Return_Conflict()
    {
        // Arrange
        Guid positionId1 = await CreatePosition();
        Guid positionId2 = await CreatePosition();
        string duplicateName = $"Дубликат_{Guid.NewGuid()}";

        // First rename position1 to the duplicate name
        using var client = this.CreateClient();
        CancellationToken cancellationToken = CancellationToken.None;

        RenamePositionRequest request1 = new(duplicateName);
        await client.PatchAsJsonAsync(
            $"/api/positions/{positionId1}",
            request1,
            cancellationToken);

        // Act - try to rename position2 to the same name
        RenamePositionRequest request2 = new(duplicateName);

        using var response = await client.PatchAsJsonAsync(
            $"/api/positions/{positionId2}",
            request2,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RenamePosition_With_Empty_Name_Should_Return_ValidationError()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        RenamePositionRequest request = new("");

        using var response = await client.PatchAsJsonAsync(
            $"/api/positions/{positionId}",
            request,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RenamePosition_With_Name_Too_Short_Should_Return_ValidationError()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        RenamePositionRequest request = new("ab");

        using var response = await client.PatchAsJsonAsync(
            $"/api/positions/{positionId}",
            request,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RenamePosition_With_Name_Too_Long_Should_Return_ValidationError()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;
        string longName = new('a', 101);

        // Act
        using var client = this.CreateClient();

        RenamePositionRequest request = new(longName);

        using var response = await client.PatchAsJsonAsync(
            $"/api/positions/{positionId}",
            request,
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
