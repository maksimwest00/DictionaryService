using CSharpFunctionalExtensions;
using DictionaryService.Application.Database;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Contracts.Departments.TransferDepartment;
using DictionaryService.Domain.Departments;
using FluentValidation;
using FluentValidation.Results;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.Kernel;

namespace DictionaryService.Application.Departments.Commands.TransferDepartment;

public class TransferDepartmentHandler : ICommandHandler<TransferDepartmentResponse, TransferDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<TransferDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public TransferDepartmentHandler(
        IDepartmentRepository departmentRepository,
        IValidator<TransferDepartmentCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<TransferDepartmentResponse, Error>> HandleAsync(
        TransferDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        // parentId не равен departmentId;
        ValidationResult? validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(
            cancellationToken,
            System.Data.IsolationLevel.Serializable);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using var transactionScope = transactionScopeResult.Value;

        // Блокируем строки в детерминированном порядке (по id ASC) для избежания deadlock
        var idsToLock = new List<Guid> { command.DepartmentId };
        if (command.Request.ParentId.HasValue)
        {
            idsToLock.Add(command.Request.ParentId.Value);
        }

        foreach (var id in idsToLock.OrderBy(x => x))
        {
            var result = await _departmentRepository.GetByIdAsync(id, cancellationToken);
            if (result.IsFailure)
            {
                transactionScope.Rollback();
                // Для parentId возвращаем Conflict, для departmentId - NotFound
                if (id == command.DepartmentId)
                {
                    return result.Error;
                }
                return Error.Conflict(
                    "department.move.parent_deleted",
                    ["Parent department not found or deleted"]);
            }
        }

        // Перечитываем данные после блокировки всех строк для получения актуального состояния
        var departmentResult = await _departmentRepository.GetByIdAsync(
            command.DepartmentId,
            cancellationToken);

        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error;
        }

        Department? newParent = null;
        if (command.Request.ParentId.HasValue)
        {
            var newParentResult = await _departmentRepository.GetByIdAsync(
                command.Request.ParentId.Value,
                cancellationToken);

            if (newParentResult.IsFailure)
            {
                transactionScope.Rollback();
                return Error.Conflict(
                    "department.move.parent_deleted",
                    ["Parent department not found or deleted"]);
            }

            newParent = newParentResult.Value;
        }

        var department = departmentResult.Value;

        // если родитель уже такой же, операция завершается как no-op без обновления БД.
        if (department.ParentId == newParent?.ParentId)
        {
            return new TransferDepartmentResponse(
                department.Id,
                department.ParentId,
                department.Path.Value,
                department.Depth,
                department.UpdatedAt);
        }

        // новый родитель не находится внутри поддерева двигаемого узла;
        if (newParent is not null)
        {
            var hasCycle = await _departmentRepository.CheckCycleAsync(
                department.Id,
                newParent.Id,
                cancellationToken);

            if (hasCycle)
            {
                transactionScope.Rollback();
                return Error.Conflict(
                    "department.move.cycle",
                    ["Cannot move department into its own subtree"]);
            }
        }

        var transferResult = await _departmentRepository.TransferAsync(
            department,
            newParent,
            cancellationToken);

        if (transferResult.IsFailure)
        {
            transactionScope.Rollback();
            return transferResult.Error;
        }

        var commitResult = transactionScope.Commit();

        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        return transferResult;
    }
}