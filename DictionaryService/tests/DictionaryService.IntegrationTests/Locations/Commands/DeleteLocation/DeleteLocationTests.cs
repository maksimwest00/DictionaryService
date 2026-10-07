using System.Net;
using System.Net.Http.Json;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.IntegrationTests.Locations.Commands.DeleteLocation;

public class DeleteLocationTests : DictionaryBaseTests
{
    public DeleteLocationTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteLocation_With_Valid_Id_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/locations/{locationId}",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            var locationWithoutFilter = await db.Locations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

            var locationWithFilter = await db.Locations
                .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotNull(locationWithoutFilter);
            Assert.False(locationWithoutFilter.IsActive);
            Assert.NotNull(locationWithoutFilter.DeletedAt);
            Assert.Null(locationWithFilter);
        });
    }

    [Fact]
    public async Task DeleteLocation_With_Non_Existent_Id_Should_Return_NotFound()
    {
        // Arrange
        Guid nonExistentLocationId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/locations/{nonExistentLocationId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_With_Invalid_Guid_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.DeleteAsync(
            $"/api/locations/{invalidGuid}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
