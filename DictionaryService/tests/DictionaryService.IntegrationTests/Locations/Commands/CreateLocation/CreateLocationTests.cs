using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Locations.CreateLocation;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.IntegrationTests.Locations.Commands.CreateLocation;

public class CreateLocationTests : DictionaryBaseTests
{
    public CreateLocationTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateLocation_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            "Локация",
            new AddressDto("Город", "Улица", "Дом", "Номер квартиры"),
            "TimeZone");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid>>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            var location = await db.Locations
                .FirstOrDefaultAsync(l => l.Id == envelope!.Result, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotEqual(Guid.Empty, envelope!.Result);
            Assert.NotNull(location);
        });
    }

    [Fact]
    public async Task CreateLocation_With_Empty_Name_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            "",
            new AddressDto("Город", "Улица", "Дом", "Номер квартиры"),
            "TimeZone");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_With_Short_Name_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            "ab",
            new AddressDto("Город", "Улица", "Дом", "Номер квартиры"),
            "TimeZone");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_With_Too_Long_Name_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            new string('a', 121),
            new AddressDto("Город", "Улица", "Дом", "Номер квартиры"),
            "TimeZone");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_With_Empty_City_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            "Локация",
            new AddressDto("", "Улица", "Дом", "Номер квартиры"),
            "TimeZone");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_With_Empty_Street_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            "Локация",
            new AddressDto("Город", "", "Дом", "Номер квартиры"),
            "TimeZone");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_With_Empty_Building_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            "Локация",
            new AddressDto("Город", "Улица", "", "Номер квартиры"),
            "TimeZone");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_With_Empty_RoomNumber_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            "Локация",
            new AddressDto("Город", "Улица", "Дом", ""),
            "TimeZone");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLocation_With_Empty_Timezone_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateLocationRequest request = new(
            "Локация",
            new AddressDto("Город", "Улица", "Дом", "Номер квартиры"),
            "");

        using var response = await client.PostAsJsonAsync(
            "/api/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
