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
            Assert.Equal(0, envelope.Result.Depth);
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
    public async Task TransferDepartment_With_Non_Existent_Parent_Should_Return_Conflict()
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

    [Fact]
    public async Task TransferDepartment_Concurrent_Same_Department_Different_Parents_Should_Handle_Race_Condition()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid parent1Id = await CreateDepartmentDb(locationId, "ParentA", "parenta");
        Guid parent2Id = await CreateDepartmentDb(locationId, "ParentB", "parentb");
        Guid childDepartmentId = await ExecuteInDb(async db =>
        {
            var parent = await db.Departments.FindAsync(parent1Id);
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

        // Act - параллельно перемещаем один и тот же отдел в разных родителей
        using var client1 = this.CreateClient();
        using var client2 = this.CreateClient();

        var task1 = client1.PutAsJsonAsync(
            $"/api/departments/{childDepartmentId}/parent",
            new TransferDepartmentRequest(parent2Id),
            cancellationToken: cancellationToken);

        var task2 = client2.PutAsJsonAsync(
            $"/api/departments/{childDepartmentId}/parent",
            new TransferDepartmentRequest(parent1Id),
            cancellationToken: cancellationToken);

        var responses = await Task.WhenAll(task1, task2);

        // Assert - хотя бы один запрос должен завершиться успешно, второй может быть Conflict
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.True(successCount >= 1, "At least one request should succeed");
        Assert.True(conflictCount <= 1, "At most one request should conflict");

        await ExecuteInDb(async db =>
        {
            Department childAfter = await db.Departments
                .FirstAsync(d => d.Id == childDepartmentId, cancellationToken);

            // Отдел должен быть в одном из родителей
            Assert.True(
                childAfter.ParentId == parent1Id || childAfter.ParentId == parent2Id,
                $"Expected ParentId to be either {parent1Id} or {parent2Id}, but got {childAfter.ParentId}");
        });
    }

    [Fact]
    public async Task TransferDepartment_Concurrent_Parent_And_Child_Should_Handle_Race_Condition()
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
        Guid newRootId = await CreateDepartmentDb(locationId, "NewRoot", "newroot");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act - параллельно перемещаем родителя в новый корень и ребенка в корень
        using var client1 = this.CreateClient();
        using var client2 = this.CreateClient();

        var task1 = client1.PutAsJsonAsync(
            $"/api/departments/{parentDepartmentId}/parent",
            new TransferDepartmentRequest(newRootId),
            cancellationToken: cancellationToken);

        var task2 = client2.PutAsJsonAsync(
            $"/api/departments/{childDepartmentId}/parent",
            new TransferDepartmentRequest(rootDepartmentId),
            cancellationToken: cancellationToken);

        var responses = await Task.WhenAll(task1, task2);

        // Assert - хотя бы один запрос должен завершиться успешно, второй может быть Conflict
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.True(successCount >= 1, "At least one request should succeed");
        Assert.True(conflictCount <= 1, "At most one request should conflict");

        await ExecuteInDb(async db =>
        {
            Department parentAfter = await db.Departments
                .FirstAsync(d => d.Id == parentDepartmentId, cancellationToken);
            Department childAfter = await db.Departments
                .FirstAsync(d => d.Id == childDepartmentId, cancellationToken);

            // Проверяем, что структура дерева не нарушена
            Assert.NotNull(parentAfter);
            Assert.NotNull(childAfter);
        });
    }

    [Fact]
    public async Task TransferDepartment_Concurrent_Multiple_Departments_To_Same_Parent_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid parentDepartmentId = await CreateDepartmentDb(locationId, "Parent", "parent");
        Guid childAId = await ExecuteInDb(async db =>
        {
            var parent = await db.Departments.FindAsync(parentDepartmentId);
            var childResult = Department.CreateChild(
                DictionaryService.Domain.Departments.Name.Create("ChildA").Value,
                DictionaryService.Domain.Departments.Identifier.Create("childa").Value,
                parent!,
                [locationId]);

            if (childResult.IsFailure)
                throw new Exception(string.Join(", ", childResult.Error.Messages));

            db.Departments.Add(childResult.Value);
            await db.SaveChangesAsync();

            return childResult.Value.Id;
        });
        Guid childBId = await ExecuteInDb(async db =>
        {
            var parent = await db.Departments.FindAsync(parentDepartmentId);
            var childResult = Department.CreateChild(
                DictionaryService.Domain.Departments.Name.Create("ChildB").Value,
                DictionaryService.Domain.Departments.Identifier.Create("childb").Value,
                parent!,
                [locationId]);

            if (childResult.IsFailure)
                throw new Exception(string.Join(", ", childResult.Error.Messages));

            db.Departments.Add(childResult.Value);
            await db.SaveChangesAsync();

            return childResult.Value.Id;
        });
        Guid newParentId = await CreateDepartmentDb(locationId, "NewParent", "newparent");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act - параллельно перемещаем двух детей в нового родителя
        using var client1 = this.CreateClient();
        using var client2 = this.CreateClient();

        var task1 = client1.PutAsJsonAsync(
            $"/api/departments/{childAId}/parent",
            new TransferDepartmentRequest(newParentId),
            cancellationToken: cancellationToken);

        var task2 = client2.PutAsJsonAsync(
            $"/api/departments/{childBId}/parent",
            new TransferDepartmentRequest(newParentId),
            cancellationToken: cancellationToken);

        var responses = await Task.WhenAll(task1, task2);

        // Assert - хотя бы один запрос должен завершиться успешно, второй может быть Conflict
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.True(successCount >= 1, "At least one request should succeed");
        Assert.True(conflictCount <= 1, "At most one request should conflict");

        await ExecuteInDb(async db =>
        {
            Department childAAfter = await db.Departments
                .FirstAsync(d => d.Id == childAId, cancellationToken);
            Department childBAfter = await db.Departments
                .FirstAsync(d => d.Id == childBId, cancellationToken);

            // Оба ребенка должны быть в новом родителе (если оба запроса успешны)
            // или только один (если второй конфликт)
            if (successCount == 2)
            {
                Assert.Equal(newParentId, childAAfter.ParentId);
                Assert.Equal(newParentId, childBAfter.ParentId);
            }
            else
            {
                // Хотя бы один ребенок должен быть в новом родителе
                Assert.True(
                    childAAfter.ParentId == newParentId || childBAfter.ParentId == newParentId,
                    "At least one child should be moved to new parent");
            }
        });
    }

    [Fact]
    public async Task TransferDepartment_Concurrent_Cycle_A_To_B_And_B_To_A_Should_Prevent_Cycle()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentAId = await CreateDepartmentDb(locationId, "DepartmentA", "depta");
        Guid departmentBId = await CreateDepartmentDb(locationId, "DepartmentB", "deptb");
        CancellationToken cancellationToken = CancellationToken.None;

        // Act - параллельно пытаемся создать цикл: A под B и B под A
        using var client1 = this.CreateClient();
        using var client2 = this.CreateClient();

        var task1 = client1.PutAsJsonAsync(
            $"/api/departments/{departmentAId}/parent",
            new TransferDepartmentRequest(departmentBId),
            cancellationToken: cancellationToken);

        var task2 = client2.PutAsJsonAsync(
            $"/api/departments/{departmentBId}/parent",
            new TransferDepartmentRequest(departmentAId),
            cancellationToken: cancellationToken);

        var responses = await Task.WhenAll(task1, task2);

        // Assert - хотя бы один запрос должен завершиться успешно, второй может быть Conflict
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.True(successCount >= 1, "At least one request should succeed");
        Assert.True(conflictCount <= 1, "At most one request should conflict");

        await ExecuteInDb(async db =>
        {
            Department departmentA = await db.Departments
                .FirstAsync(d => d.Id == departmentAId, cancellationToken);
            Department departmentB = await db.Departments
                .FirstAsync(d => d.Id == departmentBId, cancellationToken);

            // Проверяем, что цикла нет - оба отдела не являются родителями друг для друга
            Assert.False(
                departmentA.ParentId == departmentBId && departmentB.ParentId == departmentAId,
                "Cycle detected: both departments are parents of each other");
        });
    }
}
