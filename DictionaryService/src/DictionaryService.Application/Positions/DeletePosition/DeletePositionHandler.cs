using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Domain.Positions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Positions.DeletePosition;

public class DeletePositionHandler : ICommandHandler<DeletePositionCommand>
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;

    public DeletePositionHandler(
        IPositionRepository positionRepository,
        ITransactionManager transactionManager)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Error>> HandleAsync(
        DeletePositionCommand command,
        CancellationToken cancellationToken)
    {
        Guid positionId = command.Id;

        Result<Position, Error> positionResult =
            await _positionRepository.GetByIdAsync(
                positionId,
                cancellationToken);

        if (positionResult.IsFailure)
        {
            return Error.NotFound(
                null,
                ["Position not found"],
                positionId);
        }

        Position position = positionResult.Value;

        Result<ITransactionScope, Error> transactionScopeResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using ITransactionScope transactionScope = transactionScopeResult.Value;

        position.Deactivate();

        UnitResult<Error> saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }

        UnitResult<Error> commitedResult = transactionScope.Commit();

        if (commitedResult.IsFailure)
        {
            return commitedResult.Error;
        }

        return UnitResult.Success<Error>();
    }
}