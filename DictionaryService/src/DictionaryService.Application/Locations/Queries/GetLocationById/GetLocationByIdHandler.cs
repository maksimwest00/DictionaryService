using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Locations.GetLocationById;
using DictionaryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.Application.Locations.Queries.GetLocationById;

public class GetLocationByIdHandler : IQueryHandler<GetLocationByIdResponse, GetLocationByIdQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetLocationByIdHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<GetLocationByIdResponse, Error>> HandleAsync(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken)
    {
        GetLocationByIdResponse? location = await _readDbContext.LocationsRead
            .Where(x => x.Id == query.Id && x.IsActive)
            .Select(location => new GetLocationByIdResponse
            {
                Id = location.Id,
                Name = location.Name.Value,
                Address = new AddressDto(
                    location.Address.City,
                    location.Address.Street,
                    location.Address.Building,
                    location.Address.RoomNumber),
                Timezone = location.Timezone,
                IsActive = location.IsActive,
                CreatedAt = location.CreatedAt,
                UpdatedAt = location.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
        {
            return Error.NotFound(null, ["Location not found"], query.Id);
        }

        return location;
    }
}