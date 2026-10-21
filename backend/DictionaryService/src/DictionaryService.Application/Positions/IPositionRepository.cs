using CSharpFunctionalExtensions;
using DictionaryService.Domain.Positions;
using Shared.Kernel;

namespace DictionaryService.Application.Positions;

public interface IPositionRepository
{
    Task<Result<Guid, Error>> AddAsync(
        Position position,
        CancellationToken cancellationToken);

    Task<bool> IsExistPositionNameAsync(
        string positionName,
        CancellationToken cancellationToken);

    Task<Result<Position, Error>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}