using CSharpFunctionalExtensions;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Contracts.Departments.TransferDepartment;
using DictionaryService.Domain.Departments;
using FluentValidation;
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
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

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

        var idsToLock = new List<Guid> { command.DepartmentId };
        if (command.Request.ParentId.HasValue)
        {
            idsToLock.Add(command.Request.ParentId.Value);
        }

        foreach (var id in idsToLock.OrderBy(x => x))
        {
            var result = await _departmentRepository.GetByIdWithLockAsync(id, cancellationToken);
            if (result.IsFailure)
            {
                transactionScope.Rollback();
                if (id == command.DepartmentId)
                {
                    return result.Error;
                }

                return Error.Conflict(
                    "department.move.parent.deleted",
                    ["Parent department not found or deleted"]);
            }
        }

        var departmentResult = await _departmentRepository.GetByIdWithLockAsync(
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
            var newParentResult = await _departmentRepository.GetByIdWithLockAsync(
                command.Request.ParentId.Value,
                cancellationToken);

            if (newParentResult.IsFailure)
            {
                transactionScope.Rollback();
                return Error.Conflict(
                    "department.move.parent.deleted",
                    ["Parent department not found or deleted"]);
            }

            newParent = newParentResult.Value;
        }

        var department = departmentResult.Value;

        if (department.ParentId == newParent?.Id)
        {
            return new TransferDepartmentResponse(
                department.Id,
                department.ParentId,
                department.Path.Value,
                department.Depth,
                department.UpdatedAt);
        }

        if (newParent is not null)
        {
            bool hasCycle = await _departmentRepository.CheckCycleAsync(
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