using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Locations;

public interface ILocationSerivce
{
    Task<Result<Guid, Error>> CreateAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken);
}