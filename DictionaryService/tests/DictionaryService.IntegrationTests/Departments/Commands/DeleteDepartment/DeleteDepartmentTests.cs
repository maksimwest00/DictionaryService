using System.Net;
using System.Net.Http.Json;
using DictionaryService.Domain.Departments;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.IntegrationTests.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentTests : DictionaryBaseTests
{
    public DeleteDepartmentTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteDepartment_With_Valid_Id_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/departments/{departmentId}",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Department? departmentWithoutFilter = await db.Departments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

            Department? departmentWithFilter = await db.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotNull(departmentWithoutFilter);
            Assert.False(departmentWithoutFilter.IsActive);
            Assert.NotNull(departmentWithoutFilter.DeletedAt);
            Assert.Null(departmentWithFilter);
        });
    }

    [Fact]
    public async Task DeleteDepartment_With_Non_Existent_Id_Should_Return_NotFound()
    {
        // Arrange
        Guid nonExistentDepartmentId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/departments/{nonExistentDepartmentId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDepartment_With_Invalid_Guid_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.DeleteAsync(
            $"/api/departments/{invalidGuid}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDepartment_With_Empty_Guid_Should_Return_NotFound()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            "/api/departments/00000000-0000-0000-0000-000000000000",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
