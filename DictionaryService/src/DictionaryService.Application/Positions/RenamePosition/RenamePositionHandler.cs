using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Domain.Positions;
using DictionaryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace DictionaryService.Application.Positions.RenamePosition;

public class RenamePositionHandler : ICommandHandler<Guid, RenamePositionCommand>
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<RenamePositionCommand> _validator;

    public RenamePositionHandler(
        IValidator<RenamePositionCommand> validator,
        ITransactionManager transactionManager,
        IPositionRepository positionRepository)
    {
        _validator = validator;
        _transactionManager = transactionManager;
        _positionRepository = positionRepository;
    }

    public async Task<Result<Guid, Error>> HandleAsync(
        RenamePositionCommand command,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var namePosition = Name.Create(command.Request.Name).Value;

        if (await _positionRepository.IsExistPositionNameAsync(namePosition.Value, cancellationToken))
        {
            return Error.Failure(null, ["Position name is exist and active"]);
        }

        var positionResult =
            await _positionRepository.GetByIdAsync(command.Request.PositionId, cancellationToken);

        if (positionResult.IsFailure)
        {
            return positionResult.Error;
        }

        var position = positionResult.Value;

        Result<ITransactionScope, Error> transactionScopeResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using ITransactionScope transactionScope = transactionScopeResult.Value;

        position.UpdateName(namePosition);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

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

        return position.Id;
    }
}