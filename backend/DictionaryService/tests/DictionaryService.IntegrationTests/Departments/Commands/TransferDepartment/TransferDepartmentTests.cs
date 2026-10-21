using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments;
using DictionaryService.Contracts.Departments.TransferDepartment;
using DictionaryService.Domain.Departments;
using DictionaryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Framework.ResponseExtensions;

namespace DictionaryService.IntegrationTests.Departments.Commands.TransferDepartment;

public class TransferDepartmentTests : DictionaryBaseTests
{
    public TransferDepartmentTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task TransferDepartment_With_Valid_Parent_Should_Succeed()
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

        TransferDepartmentRequest request = new(parentDepartmentId);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{childDepartmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<TransferDepartmentResponse>>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Department childDepartment = await db.Departments
                .FirstAsync(d => d.Id == childDepartmentId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotNull(envelope.Result);
            Assert.Equal(parentDepartmentId, childDepartment.ParentId);
            Assert.Equal(parentDepartmentId, envelope.Result.ParentId);
        });
    }

    [Fact]
    public async Task TransferDepartment_To_Root_Should_Succeed()
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

        TransferDepartmentRequest request = new(null);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{childDepartmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<TransferDepartmentResponse>>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Department childDepartment = await db.Departments
                .FirstAsync(d => d.Id == childDepartmentId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotNull(envelope.Result);
            Assert.Null(childDepartment.ParentId);
            Assert.Equal(0, childDepartment.Depth);
            Assert.Equal(1, envelope.Result.Depth);
        });
    }

    [Fact]
    public async Task TransferDepartment_With_Children_Should_Update_All_Descendants()
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

        Guid grandchildDepartmentId = await ExecuteInDb(async db =>
        {
            var child = await db.Departments.FindAsync(childDepartmentId);
            var grandchildResult = Department.CreateChild(
                DictionaryService.Domain.Departments.Name.Create("Grandchild").Value,
                DictionaryService.Domain.Departments.Identifier.Create("grandchild").Value,
                child!,
                [locationId]);

            if (grandchildResult.IsFailure)
                throw new Exception(string.Join(", ", grandchildResult.Error.Messages));

            db.Departments.Add(grandchildResult.Value);
            await db.SaveChangesAsync();

            return grandchildResult.Value.Id;
        });

        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        TransferDepartmentRequest request = new(parentDepartmentId);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{childDepartmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Department childDepartment = await db.Departments
                .FirstAsync(d => d.Id == childDepartmentId, cancellationToken);
            Department grandchildDepartment = await db.Departments
                .FirstAsync(d => d.Id == grandchildDepartmentId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(parentDepartmentId, childDepartment.ParentId);
            Assert.Contains(childDepartment.Path.Value, grandchildDepartment.Path.Value);
        });
    }

    [Fact]
    public async Task TransferDepartment_With_Cycle_Should_Return_Conflict()
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

        TransferDepartmentRequest request = new(childDepartmentId);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{parentDepartmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.Equal("department.move.cycle", envelope.Errors.First().Code);
    }

    [Fact]
    public async Task TransferDepartment_With_Self_Parent_Should_Return_BadRequest()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        TransferDepartmentRequest request = new(departmentId);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{departmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.Equal("value.is.invalid", envelope.Errors.First().Code);
    }

    [Fact]
    public async Task TransferDepartment_With_Soft_Deleted_Parent_Should_Return_Conflict()
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
            Department parent = await db.Departments
                .FirstAsync(d => d.Id == parentDepartmentId);
            parent.Deactivate();
            await db.SaveChangesAsync();
        });

        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        TransferDepartmentRequest request = new(parentDepartmentId);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{childDepartmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.Equal("department.move.parent_deleted", envelope.Errors.First().Code);
    }

    [Fact]
    public async Task TransferDepartment_With_Non_Existent_Department_Should_Return_NotFound()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid parentDepartmentId = await CreateDepartmentDb(locationId);
        Guid nonExistentDepartmentId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        TransferDepartmentRequest request = new(parentDepartmentId);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{nonExistentDepartmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.Equal("department.not_found", envelope.Errors.First().Code);
    }

    [Fact]
    public async Task TransferDepartment_With_Non_Existent_Parent_Should_Return_NotFound()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        Guid nonExistentParentId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        TransferDepartmentRequest request = new(nonExistentParentId);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{departmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.Equal("department.move.parent_deleted", envelope.Errors.First().Code);
    }

    [Fact]
    public async Task TransferDepartment_With_Same_Parent_Should_Return_NoOp()
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

        Department child = await ExecuteInDb(async db =>
        {
            return await db.Departments
                .FirstAsync(d => d.Id == childDepartmentId, cancellationToken);
        });

        var originalUpdatedAt = child.UpdatedAt;

        // Act
        using var client = this.CreateClient();

        TransferDepartmentRequest request = new(parentDepartmentId);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{childDepartmentId}/parent",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<TransferDepartmentResponse>>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Department childAfter = await db.Departments
                .FirstAsync(d => d.Id == childDepartmentId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotNull(envelope.Result);
            Assert.Equal(originalUpdatedAt, childAfter.UpdatedAt);
        });
    }
}
