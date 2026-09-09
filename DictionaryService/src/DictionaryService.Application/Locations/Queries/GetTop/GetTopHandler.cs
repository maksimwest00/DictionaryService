using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Locations.GetTop;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Locations.Queries.GetTop;

public class GetTopHandler : IQueryHandler<GetTopResponse>
{
    private readonly IReadDbConnectionFactory _connectionFactory;


    public GetTopHandler(IReadDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<GetTopResponse, Error>> HandleAsync(CancellationToken cancellationToken)
    { 
        using IDbConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var dto = await connection.QueryAsync<LocationDto, AddressDto, LocationDto>(
            """
                SELECT  L.id
                     ,L.name
                     ,COUNT(D.id) AS DepartmentCount
                     ,L.city
                     ,L.street
                     ,L.building
                     ,L.room_number
                FROM locations AS L
                         INNER JOIN department_locations AS DL ON L.id = DL.location_id
                         INNER JOIN departments AS D ON DL.department_id = D.id
                WHERE L.is_active = true AND D."isActive" = true
                GROUP BY L.id, L.name, L.city, L.street, L.building, L.room_number
                ORDER BY DepartmentCount DESC, L.id
                LIMIT 5;
                """,
            splitOn: "city",
            map: (locationDto, addressDto) => locationDto with
            {
                Address = addressDto
            });

        var response = new GetTopResponse() { Locations = dto.ToList() };

        return response;
    }
}