using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Contracts.Departments.TransferDepartment;
using DictionaryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace DictionaryService.Application.Departments.Commands.TransferDepartment;

public class TransferDepartmentHandler : ICommandHandler<TransferDepartmentResponse, TransferDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<TransferDepartmentCommand> _validator;

    public TransferDepartmentHandler(
        IDepartmentRepository departmentRepository,
        IValidator<TransferDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
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
                return Error.NotFound(
                    null,
                    ["New parent is not exist or is not active"],
                    command.Request.ParentId.Value);
            }
        }

        // TODO
        // новый родитель не находится внутри поддерева двигаемого узла;
        // если родитель уже такой же, операция завершается как no-op без обновления БД.

        if (command.Request.ParentId.HasValue)
        {
            // Проверить, что новый parentId (если не null) существует, активен и не совпадает с departmentId
            if (command.Request.ParentId != command.DepartmentId)
            {
                var newParentDepartmentResult = await _departmentRepository.GetByIdAsync(
                    command.Request.ParentId.Value,
                    cancellationToken);

                if (newParentDepartmentResult.IsFailure)
                {
                    return Error.NotFound(
                        null,
                        ["New parent is not exist or is not active"],
                        command.DepartmentId);
                }

                // Нельзя выбрать родителем своё "дочернее" подразделение (чтобы не было зацикливания структуры)
                var isDepartmentContainsResult = await _departmentRepository.IsDepartmentContains(
                    departmentResult.Value,
                    newParentDepartmentResult.Value,
                    cancellationToken);

                if (isDepartmentContainsResult.IsFailure)
                {
                    return Error.NotFound(
                        null,
                        ["New parent is children of department"],
                        command.DepartmentId);
                }

                await _departmentRepository.TransferAsync(
                    departmentResult.Value,
                    newParentDepartmentResult.Value,
                    cancellationToken);

                return null as TransferDepartmentResponse;
            }

            return Error.NotFound(
                null,
                ["Department id equals new parent id"],
                command.DepartmentId);
        }

        await _departmentRepository.TransferAsync(
            departmentResult.Value,
            null,
            cancellationToken);

        return null as TransferDepartmentResponse;
    }
}