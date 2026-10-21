using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Domain.Departments;
using DictionaryService.Presenters.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Departments.Queries.GetChildrenByRootId;

public class GetChildrenByRootIdTests : DictionaryBaseTests
{
    public GetChildrenByRootIdTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetChildrenByRootId_With_Node_Without_Children_Should_Return_Empty_List()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{departmentId}/children",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }

    [Fact]
    public async Task GetChildrenByRootId_With_Non_Existent_Node_Should_Return_NotFound()
    {
        // Arrange
        Guid nonExistentDepartmentId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{nonExistentDepartmentId}/children",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetChildrenByRootId_With_Children_Should_Return_Them()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid parentDepartmentId = await CreateDepartmentDb(locationId, "Parent", "parent");

        Guid childDepartmentId = await ExecuteInDb(async db =>
        {
            var parent = await db.Departments.FindAsync(parentDepartmentId);
            var childResult = Department.CreateChild(
                DictionaryService.Domain.Departments.Name.Create("Child").Value,
                DictionaryService.Domain.Departments.Identifier.Create("child").Value,
                parent!,
                [locationId]);

            if (childResult.IsFailure)
                throw new Exception(string.Join(", ", childResult.Error.Messages));

            db.Departments.Add(childResult.Value);
            await db.SaveChangesAsync();

            return childResult.Value.Id;
        });

        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{parentDepartmentId}/children",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Single(envelope.Result);
        Assert.Equal(childDepartmentId, envelope.Result[0].Id);
    }

    [Fact]
    public async Task GetChildrenByRootId_Should_Not_Return_Soft_Deleted_Children()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid parentDepartmentId = await CreateDepartmentDb(locationId, "Parent", "parent");

        Guid childDepartmentId = await ExecuteInDb(async db =>
        {
            var parent = await db.Departments.FindAsync(parentDepartmentId);
            var childResult = Department.CreateChild(
                DictionaryService.Domain.Departments.Name.Create("Child").Value,
                DictionaryService.Domain.Departments.Identifier.Create("child").Value,
                parent!,
                [locationId]);

            if (childResult.IsFailure)
                throw new Exception(string.Join(", ", childResult.Error.Messages));

            db.Departments.Add(childResult.Value);
            await db.SaveChangesAsync();

            return childResult.Value.Id;
        });

        await ExecuteInDb(async db =>
        {
            var child = await db.Departments.FindAsync(childDepartmentId);
            child!.Deactivate();
            await db.SaveChangesAsync();
        });

        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{parentDepartmentId}/children",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }
}
