using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DictionaryService.Contracts.Departments;
using DictionaryService.Contracts.Departments.GetDepartmentById;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Locations;
using DictionaryService.IntegrationTests.Infrastructure;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.EntityFrameworkCore;
using Name = DictionaryService.Domain.Locations.Name;

namespace DictionaryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DictionaryBaseTests
{
    public CreateDepartmentTests(DictionaryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        Guid locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        using var client = this.CreateClient();

        CreateDepartmentRequest request = new(
            "Подразделение",
            "ppp",
            null,
            [locationId]);

        using var response = await client.PostAsJsonAsync(
            "/api/departments",
            request,
            cancellationToken: cancellationToken);


        var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid>>(cancellationToken);

        // Assert
        await ExecuteInDb(async db =>
        {
            Department department = await db.Departments
                .FirstAsync(d => d.Id == (envelope.Result), cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(envelope);
            Assert.NotEqual(Guid.Empty, envelope!.Result);
        });
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
}