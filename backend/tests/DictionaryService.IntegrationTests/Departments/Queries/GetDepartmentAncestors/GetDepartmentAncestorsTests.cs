using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Domain.Departments;
using DictionaryService.Presenters.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Departments.Queries.GetDepartmentAncestors;

public class GetDepartmentAncestorsTests : DictionaryBaseTests
{
    public GetDepartmentAncestorsTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartmentAncestors_With_Root_Node_Should_Return_Empty_List()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{departmentId}/ancestors",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }

    [Fact]
    public async Task GetDepartmentAncestors_With_Child_Node_Should_Return_Parents()
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
            $"/api/departments/{childDepartmentId}/ancestors",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Single(envelope.Result);
        Assert.Equal(parentDepartmentId, envelope.Result[0].Id);
    }

    [Fact]
    public async Task GetDepartmentAncestors_With_Nested_Child_Should_Return_All_Ancestors_In_Order()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid rootDepartmentId = await CreateDepartmentDb(locationId, "Root", "root");
        
        Guid parentDepartmentId = await ExecuteInDb(async db =>
        {
            var root = await db.Departments.FindAsync(rootDepartmentId);
            var parentResult = Department.CreateChild(
                DictionaryService.Domain.Departments.Name.Create("Parent").Value,
                DictionaryService.Domain.Departments.Identifier.Create("parent").Value,
                root!,
                [locationId]);

            if (parentResult.IsFailure)
                throw new Exception(string.Join(", ", parentResult.Error.Messages));

            db.Departments.Add(parentResult.Value);
            await db.SaveChangesAsync();

            return parentResult.Value.Id;
        });

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
            $"/api/departments/{childDepartmentId}/ancestors",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(2, envelope.Result.Count);
        Assert.Equal(rootDepartmentId, envelope.Result[0].Id);
        Assert.Equal(parentDepartmentId, envelope.Result[1].Id);
    }

    [Fact]
    public async Task GetDepartmentAncestors_Should_Not_Return_Soft_Deleted_Ancestors()
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
            var parent = await db.Departments.FindAsync(parentDepartmentId);
            parent!.Deactivate();
            await db.SaveChangesAsync();
        });

        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.GetAsync(
            $"/api/departments/{childDepartmentId}/ancestors",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<List<GetDepartmentsTreeResponse>>>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Empty(envelope.Result);
    }
}
