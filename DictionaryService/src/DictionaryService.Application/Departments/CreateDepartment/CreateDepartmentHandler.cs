using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
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
    private readonly ITransactionManager _transactionManager;

    public CreateDepartmentHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        IValidator<CreateDepartmentCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _validator = validator;
        _transactionManager = transactionManager;
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

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using var transactionScope = transactionScopeResult.Value;

        bool locationsExist = await _locationRepository.ExistsAsync(
            command.Request.LocationIds,
            cancellationToken);

        if (!locationsExist)
        {
            transactionScope.Rollback();
            return Result.Failure<Guid, Error>(Error.Failure(null, ["Locations not found"]));
        }

        if (command.Request.ParentId.HasValue)
        {
            Guid parentId = command.Request.ParentId.Value;

            var departmentParentResult =
                await _departmentRepository.GetByIdAsync(parentId, cancellationToken);

            if (departmentParentResult.IsFailure)
            {
                return departmentParentResult.Error;
            }

            Result<Department, Error> createDepartmentResult = Department.CreateChild(
                nameDepartmentResult.Value,
                identifierDepartmentResult.Value,
                departmentParentResult.Value,
                command.Request.LocationIds);

            if (createDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return createDepartmentResult.Error;
            }

            var addDepartmentResult = await _departmentRepository.AddAsync(createDepartmentResult.Value, cancellationToken);

            if (addDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return addDepartmentResult.Error;
            }

            var commitedResult = transactionScope.Commit();

            if (commitedResult.IsFailure)
            {
                return commitedResult.Error;
            }

            return addDepartmentResult;
        }
        else
        {
            Result<Department, Error> createDepartmentResult = Department.CreateParent(
                nameDepartmentResult.Value,
                identifierDepartmentResult.Value,
                command.Request.LocationIds);

            if (createDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return createDepartmentResult.Error;
            }

            var addDepartmentResult = await _departmentRepository.AddAsync(createDepartmentResult.Value, cancellationToken);

            if (addDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return addDepartmentResult.Error;
            }

            var commitedResult = transactionScope.Commit();

            if (commitedResult.IsFailure)
            {
                return commitedResult.Error;
            }

            return addDepartmentResult;
        }
    }
}