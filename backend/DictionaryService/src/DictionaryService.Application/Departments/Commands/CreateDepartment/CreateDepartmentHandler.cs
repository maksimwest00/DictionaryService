using CSharpFunctionalExtensions;
using DictionaryService.Application.Locations;
using DictionaryService.Domain.Departments;
using FluentValidation;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.Core.Validation.ValidationExtensions;
using Shared.Kernel;

namespace DictionaryService.Application.Departments.Commands.CreateDepartment;

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
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var nameDepartmentResult = Name.Create(command.Request.Name);

        var identifierDepartmentResult = Identifier.Create(command.Request.Identifier);

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
            return Error.NotFound("locations.not.found", ["Locations not found"], null);
        }

        if (command.Request.ParentId.HasValue)
        {
            var parentId = command.Request.ParentId.Value;

            var departmentParentResult =
                await _departmentRepository.GetByIdAsync(parentId, cancellationToken);

            if (departmentParentResult.IsFailure)
            {
                return departmentParentResult.Error;
            }

            var createDepartmentResult = Department.CreateChild(
                nameDepartmentResult.Value,
                identifierDepartmentResult.Value,
                departmentParentResult.Value,
                command.Request.LocationIds);

            if (createDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return createDepartmentResult.Error;
            }

            var addDepartmentResult =
                await _departmentRepository.AddAsync(createDepartmentResult.Value, cancellationToken);

            if (addDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return addDepartmentResult.Error;
            }

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
            {
                transactionScope.Rollback();
                return saveResult.Error;
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
            var createDepartmentResult = Department.CreateParent(
                nameDepartmentResult.Value,
                identifierDepartmentResult.Value,
                command.Request.LocationIds);

            if (createDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return createDepartmentResult.Error;
            }

            var addDepartmentResult =
                await _departmentRepository.AddAsync(createDepartmentResult.Value, cancellationToken);

            if (addDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return addDepartmentResult.Error;
            }

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
            {
                transactionScope.Rollback();
                return saveResult.Error;
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