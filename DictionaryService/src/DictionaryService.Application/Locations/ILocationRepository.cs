using DictionaryService.Domain.Locations;

namespace DictionaryService.Application.Locations;

public interface ILocationRepository
{
    Task<Guid> AddAsync(Location location, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid[] locationIds, CancellationToken cancellationToken);

    Task<bool> ExistsAndActiveAsync(Guid[] locationIds, CancellationToken cancellationToken);
}