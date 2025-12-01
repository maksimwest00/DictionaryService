using DictionaryService.Contracts.Locations;
using DictionaryService.Domain.Locations;
using Name = DictionaryService.Domain.Locations.Name;

namespace DictionaryService.Application.Locations;

public class LocationSerivce : ILocationSerivce
{
    private readonly ILocationRepository _locationRepository;

    public LocationSerivce(ILocationRepository locationRepository) => _locationRepository = locationRepository;

    public async Task<Guid> CreateAsync(
        CreateLocationDto request,
        CancellationToken cancellationToken)
    {
        Name nameLocation = Name.Create(request.Name);

        Address addressLocation = Address.Create(
            request.Address.City,
            request.Address.Street,
            request.Address.Building,
            request.Address.RoomNumber);

        Location location = new(
            nameLocation,
            addressLocation,
            request.Timezone);

        return await _locationRepository.AddAsync(location, cancellationToken);
    }
}