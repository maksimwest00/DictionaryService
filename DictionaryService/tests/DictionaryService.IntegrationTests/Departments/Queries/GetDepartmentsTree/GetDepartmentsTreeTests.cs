using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Departments.Queries.GetDepartmentsTree;

public class GetDepartmentsTreeTests : DictionaryBaseTests
{
    public GetDepartmentsTreeTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartmentsTree_With_Empty_Tree_Should_Return_Empty_List()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/tree",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }

    [Fact]
    public async Task GetDepartmentsTree_With_Root_Departments_Should_Return_Them()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/tree",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Single(envelope.Result);
        Assert.Equal(departmentId, envelope.Result[0].Id);
    }

    [Fact]
    public async Task GetDepartmentsTree_Should_Not_Return_Soft_Deleted_Departments()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);

        await ExecuteInDb(async db =>
        {
            var department = await db.Departments.FindAsync(departmentId);
            department!.Deactivate();
            await db.SaveChangesAsync();
        });

        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/tree",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }
}
