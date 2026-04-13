using CSharpFunctionalExtensions;
using DictionaryService.Domain.Positions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Positions;

public interface IPositionRepository
{
    Task<Result<Guid, Error>> AddAsync(
        Position position,
        CancellationToken cancellationToken);

    Task<bool> IsExistPositionNameAsync(
        string positionName,
        CancellationToken cancellationToken);
}