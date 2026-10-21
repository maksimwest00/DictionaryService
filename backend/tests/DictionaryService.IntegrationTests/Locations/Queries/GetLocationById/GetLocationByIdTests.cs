using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Locations.GetLocationById;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Locations.Queries.GetLocationById;

public class GetLocationByIdTests : DictionaryBaseTests
{
    public GetLocationByIdTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLocationById_With_Valid_Id_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/locations/{locationId}",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetLocationByIdResponse>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.Equal(locationId, result.Id);
    }

    [Fact]
    public async Task GetLocationById_With_Non_Existent_Id_Should_Return_NotFound()
    {
        // Arrange
        Guid nonExistentLocationId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/locations/{nonExistentLocationId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLocationById_With_Invalid_Guid_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.GetAsync(
            $"/api/locations/{invalidGuid}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLocationById_Should_Return_Location_With_All_Fields()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/locations/{locationId}",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetLocationByIdResponse>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(locationId, envelope.Result.Id);
        Assert.NotEmpty(envelope.Result.Name);
        Assert.NotNull(envelope.Result.Address);
        Assert.NotEmpty(envelope.Result.Address.City);
        Assert.NotEmpty(envelope.Result.Address.Street);
        Assert.NotEmpty(envelope.Result.Address.Building);
        Assert.NotEmpty(envelope.Result.Address.RoomNumber);
        Assert.NotEmpty(envelope.Result.Timezone);
        Assert.True(envelope.Result.IsActive);
        Assert.True(envelope.Result.CreatedAt > DateTime.MinValue);
        Assert.True(envelope.Result.UpdatedAt > DateTime.MinValue);
    }
}
