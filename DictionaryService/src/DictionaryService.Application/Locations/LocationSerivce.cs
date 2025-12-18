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
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var nameLocationResult = Name.Create(command.Request.Name);

        if (nameLocationResult.IsFailure)
            return nameLocationResult.Error;

        var addressLocationResult = Address.Create(
            command.Request.Address.City,
            command.Request.Address.Street,
            command.Request.Address.Building,
            command.Request.Address.RoomNumber);

        if (addressLocationResult.IsFailure)
            return addressLocationResult.Error;

        Location location = new(
            nameLocationResult.Value,
            addressLocationResult.Value,
            command.Request.Timezone);

        return await _locationRepository.AddAsync(location, cancellationToken);
    }
}