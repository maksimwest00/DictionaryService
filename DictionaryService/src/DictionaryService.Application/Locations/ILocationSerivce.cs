using DictionaryService.Contracts.Locations;

namespace DictionaryService.Application.Locations;

public interface ILocationSerivce
{
    Task<Guid> CreateAsync(
        CreateLocationDto request,
        CancellationToken cancellationToken);
}