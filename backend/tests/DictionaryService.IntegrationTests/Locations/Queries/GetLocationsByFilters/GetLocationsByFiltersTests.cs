using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Locations.GetLocationsByFilters;
using DictionaryService.Contracts.Shared;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Locations.Queries.GetLocationsByFilters;

public class GetLocationsByFiltersTests : DictionaryBaseTests
{
    public GetLocationsByFiltersTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLocationsByFilters_With_Default_Params_Should_Succeed()
    {
        // Arrange
        await CreateLocationDb("Локация1");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
    }

    [Fact]
    public async Task GetLocationsByFilters_With_Invalid_PageSize_Should_Return_ValidationError()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations?pageSize=0",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetLocationsByFilters_With_Search_Should_Filter_Results()
    {
        // Arrange
        await CreateLocationDb("Локация1");
        await CreateLocationDb("Локация2");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations?search=Локация1",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Contains("Локация1", result.Data.First().Name);
    }

    [Fact]
    public async Task GetLocationsByFilters_With_Pagination_Should_Return_Correct_Page()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await CreateLocationDb($"Локация{i + 1}");
        }
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations?page=1&pageSize=2",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetLocationsByFilters_With_SortBy_Should_Order_Results()
    {
        // Arrange
        await CreateLocationDb("B_Location");
        await CreateLocationDb("A_Location");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations?sortBy=name&sortDir=asc",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.True(result.Data.Count >= 2);
        Assert.Equal("A_Location", result.Data.First().Name);
    }

    [Fact]
    public async Task GetLocationsByFilters_With_MinDepartmentCount_Should_Filter_Results()
    {
        // Arrange
        Guid locationId1 = await CreateLocationDb("Локация1");
        Guid locationId2 = await CreateLocationDb("Локация2");
        
        // Create departments for location1
        await CreateDepartmentApi(locationId1, "Подразделение1", "pppa");
        await CreateDepartmentApi(locationId1, "Подразделение2", "pppb");
        
        // Create one department for location2
        await CreateDepartmentApi(locationId2, "Подразделение3", "pppc");
        
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations?minDepartmentCount=2",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemResponse>>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.All(result.Data, loc => Assert.True(loc.DepartmentCount >= 2));
    }
}
