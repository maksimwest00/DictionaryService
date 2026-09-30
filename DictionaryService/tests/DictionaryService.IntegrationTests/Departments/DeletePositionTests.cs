using System.Net;
using System.Net.Http.Json;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Locations;
using DictionaryService.Domain.Positions;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;
using Name = DictionaryService.Domain.Locations.Name;

namespace DictionaryService.IntegrationTests.Departments;

public class DeletePositionTests : DictionaryBaseTests
{
    public DeletePositionTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeletePosition_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        Guid departmentId = await CreateDepartment(locationId);
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        using var client = this.CreateClient();

        // First add position
        await client.PostAsync(
            $"/api/departments/{departmentId}/positions/{positionId}",
            null,
            cancellationToken);

        // Act
        using var response = await client.DeleteAsync(
            $"/api/departments/{departmentId}/positions/{positionId}",
            cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            var departmentPosition = await db.DepartmentPositions
                .FirstOrDefaultAsync(dp => dp.DepartmentId == departmentId && dp.PositionId == positionId, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.Null(departmentPosition);
        });
    }

    [Fact]
    public async Task DeletePosition_With_Non_Existent_Department_Should_Return_NotFound()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        Guid nonExistentDepartmentId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/departments/{nonExistentDepartmentId}/positions/{positionId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePosition_With_Non_Existent_Position_Should_Return_NotFound()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        Guid departmentId = await CreateDepartment(locationId);
        Guid nonExistentPositionId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/departments/{departmentId}/positions/{nonExistentPositionId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePosition_When_Not_Added_Should_Return_Conflict()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        Guid departmentId = await CreateDepartment(locationId);
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act - try to delete without adding first
        using var client = this.CreateClient();

        using var response = await client.DeleteAsync(
            $"/api/departments/{departmentId}/positions/{positionId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePosition_With_Invalid_Department_Guid_Should_Return_BadRequest()
    {
        // Arrange
        Guid positionId = await CreatePosition();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.DeleteAsync(
            $"/api/departments/{invalidGuid}/positions/{positionId}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePosition_With_Invalid_Position_Guid_Should_Return_BadRequest()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        Guid departmentId = await CreateDepartment(locationId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        var invalidGuid = Guid.Empty;

        using var response = await client.DeleteAsync(
            $"/api/departments/{departmentId}/positions/{invalidGuid}",
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> CreateLocation() =>
        await ExecuteInDb(async db =>
        {
            Location location = new(
                Name.Create("Локация").Value,
                Address.Create(
                    "Город",
                    "Улица",
                    "Дом",
                    "Номер квартиры").Value,
                "TimeZone");

            db.Locations.Add(location);
            await db.SaveChangesAsync();

            return location.Id;
        });

    private async Task<Guid> CreateDepartment(Guid locationId) =>
        await ExecuteInDb(async db =>
        {
            var departmentResult = Department.CreateParent(
                DictionaryService.Domain.Departments.Name.Create("Подразделение").Value,
                DictionaryService.Domain.Departments.Identifier.Create("ppp").Value,
                [locationId]);

            if (departmentResult.IsFailure)
                throw new Exception(string.Join(", ", departmentResult.Error.Messages));

            db.Departments.Add(departmentResult.Value);
            await db.SaveChangesAsync();

            return departmentResult.Value.Id;
        });

    private async Task<Guid> CreatePosition() =>
        await ExecuteInDb(async db =>
        {
            Position position = Position.Create(
                DictionaryService.Domain.Positions.Name.Create("Должность").Value,
                DictionaryService.Domain.Positions.Description.Create("Описание").Value,
                []);

            db.Positions.Add(position);
            await db.SaveChangesAsync();

            return position.Id;
        });
}
