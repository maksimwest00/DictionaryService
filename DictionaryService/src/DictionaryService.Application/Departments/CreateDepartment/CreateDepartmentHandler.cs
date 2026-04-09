using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Locations;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace DictionaryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IValidator<CreateDepartmentCommand> _validator;

    public CreateDepartmentHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        IValidator<CreateDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _validator = validator;
    }

    public async Task<Result<Guid, Error>> HandleAsync(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        Result<Name, Error> nameDepartmentResult = Name.Create(command.Request.Name);

        Result<Identifier, Error> identifierDepartmentResult = Identifier.Create(command.Request.Identifier);

        bool locationsExist = await _locationRepository.ExistsAsync(
            command.Request.LocationIds,
            cancellationToken);

        if (!locationsExist)
        {
            return Result.Failure<Guid, Error>(Error.Failure(null, ["Locations not found"]));
        }

        if (!command.Request.ParentId.HasValue)
        {
            Result<Department, Error> createDepartmentResult = Department.CreateParent(
                nameDepartmentResult.Value,
                identifierDepartmentResult.Value,
                command.Request.LocationIds);

            if (createDepartmentResult.IsFailure)
            {
                return createDepartmentResult.Error;
            }

            return await _departmentRepository.AddAsync(createDepartmentResult.Value, cancellationToken);
        }
        else
        {
            Guid parentId = command.Request.ParentId.Value;

            Department? departmentParent =
                await _departmentRepository.GetByIdAsync(parentId, cancellationToken);

            if (departmentParent is null)
            {
                return Result.Failure<Guid, Error>(Error.NotFound(
                    null,
                    ["Department parent not found"],
                    parentId));
            }

            Result<Department, Error> createDepartmentResult = Department.CreateChild(
                nameDepartmentResult.Value,
                identifierDepartmentResult.Value,
                departmentParent,
                command.Request.LocationIds);

            if (createDepartmentResult.IsFailure)
            {
                return createDepartmentResult.Error;
            }

            return await _departmentRepository.AddAsync(createDepartmentResult.Value, cancellationToken);
        }
    }
}