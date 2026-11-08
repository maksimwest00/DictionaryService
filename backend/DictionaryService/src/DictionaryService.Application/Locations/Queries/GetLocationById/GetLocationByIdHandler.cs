using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Locations.GetLocationById;
using Shared.Core.Abstractions;
using Shared.Kernel;

namespace DictionaryService.Application.Locations.Queries.GetLocationById;

public record LocationDto(
    Guid Id,
    string Name,
    string City,
    string Street,
    string Building,
    string RoomNumber,
    string Timezone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class GetLocationByIdHandler : IQueryHandler<GetLocationByIdResponse, GetLocationByIdQuery>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetLocationByIdHandler(IReadDbConnectionFactory readDbConnectionFactory)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<GetLocationByIdResponse, Error>> HandleAsync(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var location = await connection.QueryFirstOrDefaultAsync<dynamic>(
            """
                    SELECT
                        id,
                        name,
                        city,
                        street,
                        building,
                        room_number,
                        timezone,
                        is_active,
                        created_at,
                        updated_at
                    FROM locations
                    WHERE id = @Id
                      AND is_active = true
                """,
            param: new { Id = query.Id });

        if (location is null)
        {
            return Error.NotFound(
                "location.not.found",
                ["Location not found"],
                query.Id);
        }

        var response = new GetLocationByIdResponse
        {
            Id = location.id,
            Name = location.name,
            Address = new AddressDto(
                location.city,
                location.street,
                location.building,
                location.room_number),
            Timezone = location.timezone,
            IsActive = location.is_active,
            CreatedAt = location.created_at,
            UpdatedAt = location.updated_at,
        };

        return response;
    }
}