using System.Net;
using System.Net.Http.Json;
using DictionaryService.Contracts.Departments;
using DictionaryService.Domain.Departments;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.IntegrationTests.Departments.Commands.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsTests : DictionaryBaseTests
{
    public UpdateDepartmentLocationsTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateDepartmentLocations_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        Guid locationId1 = await CreateLocationDb();
        Guid locationId2 = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId1);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        UpdateDepartmentLocationsRequest request = new([locationId2]);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{departmentId}/locations",
            request,
            cancellationToken: cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Department department = await db.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == departmentId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.Single(department.DepartmentLocations);
            Assert.Equal(locationId2, department.DepartmentLocations.First().LocationId);
        });
    }

    [Fact]
    public async Task UpdateDepartmentLocations_With_Non_Existent_Department_Should_Return_NotFound()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid nonExistentDepartmentId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        UpdateDepartmentLocationsRequest request = new([locationId]);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{nonExistentDepartmentId}/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_With_Empty_Locations_Should_Return_ValidationError()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        UpdateDepartmentLocationsRequest request = new(Array.Empty<Guid>());

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{departmentId}/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_With_Non_Existent_Location_Should_Return_NotFound()
    {
        // Arrange
        Guid locationId = await CreateLocationDb();
        Guid departmentId = await CreateDepartmentDb(locationId);
        Guid nonExistentLocationId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        UpdateDepartmentLocationsRequest request = new([nonExistentLocationId]);

        using var response = await client.PutAsJsonAsync(
            $"/api/departments/{departmentId}/locations",
            request,
            cancellationToken: cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
