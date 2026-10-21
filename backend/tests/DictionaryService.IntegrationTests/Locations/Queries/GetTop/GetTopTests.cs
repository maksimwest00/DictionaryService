using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Locations.GetTop;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Locations.Queries.GetTop;

public class GetTopTests : DictionaryBaseTests
{
    public GetTopTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTop_Should_Return_Top_Locations()
    {
        // Arrange
        Guid locationId1 = await CreateLocationDb("Локация1");
        Guid locationId2 = await CreateLocationDb("Локация2");
        await CreateDepartmentApi(locationId1, "Подразделение1", "pppa");
        await CreateDepartmentApi(locationId2, "Подразделение2", "pppb");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations/top",
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.NotEmpty(content);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetTopResponse>>(cancellationToken);
        var result = envelope?.Result;

        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.NotNull(result.Locations);
        Assert.True(result.Locations.Count > 0);
    }

    [Fact]
    public async Task GetTop_With_No_Departments_Should_Return_Empty_List()
    {
        // Arrange
        await CreateLocationDb("Локация1");
        await CreateLocationDb("Локация2");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations/top",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetTopResponse>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.NotNull(result.Locations);
        Assert.Empty(result.Locations);
    }

    [Fact]
    public async Task GetTop_Should_Return_Max_5_Locations()
    {
        // Arrange
        char c = 'a';
        for (int i = 0; i < 7; i++)
        {
            Guid locationId = await CreateLocationDb($"Локация{i}");
            await CreateDepartmentApi(locationId, $"Подразделение{i}", $"ppp{c}");
            c++;
        }
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations/top",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetTopResponse>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.NotNull(result.Locations);
        Assert.True(result.Locations.Count <= 5);
    }

    [Fact]
    public async Task GetTop_Should_Order_By_DepartmentCount_Desc()
    {
        // Arrange
        Guid locationId1 = await CreateLocationDb("Локация1");
        Guid locationId2 = await CreateLocationDb("Локация2");
        Guid locationId3 = await CreateLocationDb("Локация3");
        
        await CreateDepartmentApi(locationId1, "Подразделение1", "pppa");
        await CreateDepartmentApi(locationId1, "Подразделение2", "pppb");
        await CreateDepartmentApi(locationId1, "Подразделение3", "pppc");
        
        await CreateDepartmentApi(locationId2, "Подразделение4", "pppd");
        await CreateDepartmentApi(locationId2, "Подразделение5", "pppe");
        
        await CreateDepartmentApi(locationId3, "Подразделение6", "pppf");
        
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/locations/top",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetTopResponse>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.NotNull(result.Locations);
        Assert.True(result.Locations.Count >= 3);
        
        // Check ordering by department count
        Assert.True(result.Locations[0].DepartmentCount >= result.Locations[1].DepartmentCount);
        Assert.True(result.Locations[1].DepartmentCount >= result.Locations[2].DepartmentCount);
    }
}
