using DictionaryService.Domain.Departments;

namespace DictionaryService.Application.Locations;

public interface ILocationRepository
{
    Task<Guid> AddAsync(Location location, CancellationToken cancellationToken);
}