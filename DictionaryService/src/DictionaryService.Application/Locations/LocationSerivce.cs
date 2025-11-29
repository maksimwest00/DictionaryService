using DictionaryService.Contracts.Locations;
using DictionaryService.Domain.Departments;

namespace DictionaryService.Application.Locations;

public class LocationSerivce : ILocationSerivce
{
    private readonly ILocationRepository _locationRepository;

    public LocationSerivce(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Guid> CreateAsync(
        CreateLocationDto request,
        CancellationToken cancellationToken)
    {
        var nameLocation = Name.Create(request.Name);

        var addressLocation = Address.Create(
            request.Address.City,
            request.Address.Street,
            request.Address.Building,
            request.Address.RoomNumber);

        var location = new Location(
            nameLocation,
            addressLocation,
            request.Timezone);

        return await _locationRepository.AddAsync(location, cancellationToken);
    }
}