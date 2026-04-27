using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Departments.TransferDepartment;

public class TransferDepartmentHandler : ICommandHandler<Guid, TransferDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;

    public TransferDepartmentHandler(
        IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<Guid, Error>> HandleAsync(
        TransferDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        // Проверить, что существует ли подразделение с таким departmentId и оно активно
        var department = await _departmentRepository.GetByIdAsync(
            command.DepartmentId,
            cancellationToken);

        if (department == null)
        {
            return Result.Failure<Guid, Error>(Error.NotFound(
                null,
                ["Department not found"],
                command.DepartmentId));
        }

        if (command.Request.ParentId.HasValue)
        {
            // Проверить, что новый parentId (если не null) существует, активен и не совпадает с departmentId
            if (command.Request.ParentId != command.DepartmentId)
            {
                var newParentDepartment = await _departmentRepository.GetByIdAsync(
                    command.Request.ParentId.Value,
                    cancellationToken);

                if (newParentDepartment != null)
                {
                    // Нельзя выбрать родителем своё "дочернее" подразделение (чтобы не было зацикливания структуры)
                    var isDepartmentContainsResult = await _departmentRepository.IsDepartmentContains(
                        department,
                        newParentDepartment,
                        cancellationToken);

                    if (isDepartmentContainsResult.IsFailure)
                    {
                        return Result.Failure<Guid, Error>(Error.NotFound(
                            null,
                            ["New parent is children of department"],
                            command.DepartmentId));
                    }

                    await _departmentRepository.TransferAsync(
                        department,
                        newParentDepartment,
                        cancellationToken);

                    return command.DepartmentId;

                }

                return Result.Failure<Guid, Error>(Error.NotFound(
                    null,
                    ["New parent is not exist or is not active"],
                    command.DepartmentId));
            }

            return Result.Failure<Guid, Error>(Error.NotFound(
                null,
                ["Department id equals new parent id"],
                command.DepartmentId));
        }

        await _departmentRepository.TransferAsync(
            department,
            null,
            cancellationToken);

        return command.DepartmentId;

    }
}