using System.Net;
using DictionaryService.Domain.Positions;
using DictionaryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.IntegrationTests.Positions.DeletePosition;

public class DeletePositionTests : DictionaryBaseTests
{
    public DeletePositionTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeletePosition_With_Valid_Id_Should_Succeed()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/positions/{positionId}",
            cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Position? position = await db.Positions
                .FirstOrDefaultAsync(p => p.Id == positionId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(position);
            Assert.False(position.IsActive);
        });
    }

    [Fact]
    public async Task DeletePosition_With_Non_Existent_Id_Should_Return_NotFound()
    {
        // Arrange
        Guid nonExistentPositionId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/positions/{nonExistentPositionId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePosition_With_Invalid_Guid_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.DeleteAsync(
            $"/api/positions/{invalidGuid}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
