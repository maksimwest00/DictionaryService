using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
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
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;
    private readonly IValidator<TransferDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public TransferDepartmentHandler(
        IDepartmentRepository departmentRepository,
        IReadDbConnectionFactory readDbConnectionFactory,
        IValidator<TransferDepartmentCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _readDbConnectionFactory = readDbConnectionFactory;
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

        // двигаемое подразделение существует и не soft-deleted;
        var departmentResult = await _departmentRepository.GetByIdAsync(
            command.DepartmentId,
            cancellationToken);

        if (departmentResult.IsFailure)
        {
            return departmentResult.Error;
        }

        // новый родитель либо null, либо существует и не soft-deleted;
        if (command.Request.ParentId.HasValue)
        {
            if (!await _departmentRepository.ExistsAndActiveAsync(command.Request.ParentId.Value, cancellationToken))
            {
                return Error.Conflict(
                    "department.move.parent_deleted",
                    ["Parent department is soft-deleted"]);
            }
        }

        // если родитель уже такой же, операция завершается как no-op без обновления БД.
        if (command.Request.ParentId.HasValue &&
            departmentResult.Value.ParentId == command.Request.ParentId.Value)
        {
            return new TransferDepartmentResponse(
                departmentResult.Value.Id,
                departmentResult.Value.ParentId,
                departmentResult.Value.Path.Value,
                departmentResult.Value.Depth,
                departmentResult.Value.UpdatedAt);
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using var transactionScope = transactionScopeResult.Value;

        // новый родитель не находится внутри поддерева двигаемого узла;
        if (command.Request.ParentId.HasValue)
        {
            using IDbConnection connection =
                await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

            var cond = await connection.ExecuteScalarAsync<int>(
                """
                    -- получаем путь двигаемоего департамента
                    WITH parent AS (
                        SELECT path
                        FROM departments
                        WHERE id = @departmentId
                    ),
                    -- получаем список его детей
                    childrens AS (
                        SELECT *
                        FROM departments
                        WHERE path::ltree <@ (SELECT path FROM parent)
                          AND path::ltree <> (SELECT path FROM parent)
                    )

                    -- смотрим есть ли среди детей тот департамент в который мы перемещаем
                    SELECT 1
                    FROM childrens
                    WHERE id = @parentId
                """,
                new
                {
                    departmentId = command.DepartmentId,
                    parentId = command.Request.ParentId,
                } ) == 1;

            if (cond)
            {
                transactionScope.Rollback();
                return Error.Conflict(
                    "department.move.cycle",
                    ["Cannot move department into its own subtree"]);
            }
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
                return Error.NotFound(
                    "department.parent.not_found",
                    ["Parent department not found"],
                    command.Request.ParentId.Value);
            }

            newParent = newParentResult.Value;
        }

        var transferResult = await _departmentRepository.TransferAsync(
            departmentResult.Value,
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