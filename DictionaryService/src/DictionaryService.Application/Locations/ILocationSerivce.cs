using CSharpFunctionalExtensions;
using DictionaryService.Contracts.Locations;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Locations;

public interface ILocationSerivce
{
    Task<Result<Guid, Error>> CreateAsync(
        CreateLocationDto request,
        CancellationToken cancellationToken);
}