using CSharpFunctionalExtensions;
using DictionaryService.Contracts.Locations;
using DictionaryService.Domain.Locations;
using DictionaryService.Domain.Shared;
using Name = DictionaryService.Domain.Locations.Name;

namespace DictionaryService.Application.Locations;

public class LocationSerivce : ILocationSerivce
{
    private readonly ILocationRepository _locationRepository;

    public LocationSerivce(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<Guid, Error>> CreateAsync(
        CreateLocationDto request,
        CancellationToken cancellationToken)
    {
        var nameLocationResult = Name.Create(request.Name);

        if (nameLocationResult.IsFailure)
            return nameLocationResult.Error;

        var addressLocationResult = Address.Create(
            request.Address.City,
            request.Address.Street,
            request.Address.Building,
            request.Address.RoomNumber);

        if (addressLocationResult.IsFailure)
            return addressLocationResult.Error;

        Location location = new(
            nameLocationResult.Value,
            addressLocationResult.Value,
            request.Timezone);

        return await _locationRepository.AddAsync(location, cancellationToken);
    }
}