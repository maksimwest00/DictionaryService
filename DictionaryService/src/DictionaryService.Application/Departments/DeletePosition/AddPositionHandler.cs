using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Application.Departments.DeletePosition;
using DictionaryService.Application.Positions;
using DictionaryService.Domain.DepartmentPositions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Departments.DeletePosition;

public class DeletePositionHandler : ICommandHandler<Guid, DeletePositionCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;

    public DeletePositionHandler(
        IDepartmentRepository departmentRepository,
        IPositionRepository positionRepository,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> HandleAsync(
        DeletePositionCommand command,
        CancellationToken cancellationToken)
    {
        var departmentId = command.DeptId;

        var departmentResult =
            await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);

        if (departmentResult.IsFailure)
        {
            return departmentResult.Error;
        }

        var positionId = command.PosId;

        var positionResult =
            await _positionRepository.GetByIdAsync(positionId, cancellationToken);

        if (positionResult.IsFailure)
        {
            return positionResult.Error;
        }

        var departmentPosition = new DepartmentPosition(departmentId, positionId);

        Result<ITransactionScope, Error> transactionScopeResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using ITransactionScope transactionScope = transactionScopeResult.Value;

        var addPositionResult =
            await _departmentRepository.DeletePositionAsync(departmentPosition, cancellationToken);

        if (addPositionResult.IsFailure)
        {
            transactionScope.Rollback();
            return addPositionResult.Error;
        }

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

        return departmentId;
    }
}