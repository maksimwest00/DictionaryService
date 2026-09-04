using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Departments.DeleteDepartment;

public class DeleteDepartmentHandler : ICommandHandler<DeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;

    public DeleteDepartmentHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Error>> HandleAsync(
        DeleteDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var departmentId = command.Id;

        var departmentResult =
            await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);

        if (departmentResult.IsFailure)
        {
            return departmentResult.Error;
        }

        var department = departmentResult.Value;

        Result<ITransactionScope, Error> transactionScopeResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using ITransactionScope transactionScope = transactionScopeResult.Value;

        department.Deactivate();

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