using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Departments.Queries.GetDepartmentsTreeBySearch;

public class GetDepartmentsTreeBySearchTests : DictionaryBaseTests
{
    public GetDepartmentsTreeBySearchTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartmentsTreeBySearch_With_No_Results_Should_Return_Empty_List()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        await CreateDepartmentDb(locationId, "Подразделение", "dept");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/tree/search?search=NonExistent",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }

    [Fact]
    public async Task GetDepartmentsTreeBySearch_With_Short_Query_Should_Return_Empty_List()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        await CreateDepartmentDb(locationId, "Подразделение", "dept");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/tree/search?search=a",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }

    [Fact]
    public async Task GetDepartmentsTreeBySearch_With_Matching_Name_Should_Return_Results()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId, "Подразделение", "dept");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/tree/search?search=Подразделение",
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
    public async Task GetDepartmentsTreeBySearch_With_Partial_Match_Should_Return_Results()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId, "Подразделение", "dept");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/tree/search?search=под",
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
    public async Task GetDepartmentsTreeBySearch_Should_Not_Return_Soft_Deleted_Departments()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId, "Подразделение", "dept");

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
            "/api/departments/tree/search?search=Подразделение",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }

    [Fact]
    public async Task GetDepartmentsTreeBySearch_Should_Not_Return_Non_Root_Departments()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid parentDepartmentId = await CreateDepartmentDb(locationId, "Parent", "parent");

        await ExecuteInDb(async db =>
        {
            var parent = await db.Departments.FindAsync(parentDepartmentId);
            var childResult = DictionaryService.Domain.Departments.Department.CreateChild(
                DictionaryService.Domain.Departments.Name.Create("Child").Value,
                DictionaryService.Domain.Departments.Identifier.Create("child").Value,
                parent!,[locationId]);

            if (childResult.IsFailure)
                throw new Exception(string.Join(", ", childResult.Error.Messages));

            db.Departments.Add(childResult.Value);
            await db.SaveChangesAsync();
        });

        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            "/api/departments/tree/search?search=Child",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }
}
