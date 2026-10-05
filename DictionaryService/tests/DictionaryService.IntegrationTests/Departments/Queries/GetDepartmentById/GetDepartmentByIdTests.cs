using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments.GetDepartmentById;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Departments.Queries.GetDepartmentById;

public class GetDepartmentByIdTests : DictionaryBaseTests
{
    public GetDepartmentByIdTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartmentById_With_Valid_Id_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{departmentId}",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetDepartmentByIdResponse>>(cancellationToken);
        var result = envelope?.Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(result);
        Assert.Equal(departmentId, result.Id);
    }

    [Fact]
    public async Task GetDepartmentById_With_Non_Existent_Id_Should_Return_NotFound()
    {
        // Arrange
        Guid nonExistentDepartmentId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{nonExistentDepartmentId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentById_With_Invalid_Guid_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.GetAsync(
            $"/api/departments/{invalidGuid}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentById_With_Empty_Guid_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/00000000-0000-0000-0000-000000000000",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartmentById_Should_Return_Department_With_All_Fields()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{departmentId}",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetDepartmentByIdResponse>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(departmentId, envelope.Result.Id);
        Assert.NotEmpty(envelope.Result.Name);
        Assert.NotEmpty(envelope.Result.Identifier);
        Assert.NotNull(envelope.Result.Path);
        Assert.True(envelope.Result.Depth >= 0);
        Assert.True(envelope.Result.ChildrenCount >= 0);
        Assert.True(envelope.Result.IsActive);
        Assert.True(envelope.Result.CreatedAt > DateTime.MinValue);
        Assert.True(envelope.Result.UpdatedAt > DateTime.MinValue);
    }
}
