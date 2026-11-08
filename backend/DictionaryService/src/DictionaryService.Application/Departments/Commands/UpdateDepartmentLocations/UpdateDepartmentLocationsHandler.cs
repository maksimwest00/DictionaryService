using CSharpFunctionalExtensions;
using DictionaryService.Application.Locations;
using DictionaryService.Domain.DepartmentLocations;
using FluentValidation;
using FluentValidation.Results;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.Core.Validation.ValidationExtensions;
using Shared.Kernel;

namespace DictionaryService.Application.Departments.Commands.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsHandler : ICommandHandler<Guid, UpdateDepartmentLocationsCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public UpdateDepartmentLocationsHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        IValidator<UpdateDepartmentLocationsCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> HandleAsync(
        UpdateDepartmentLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        bool existDepartmentAndActive =
            await _departmentRepository.ExistsAndActiveAsync(
                command.DepartmentId,
                cancellationToken);

        if (!existDepartmentAndActive)
        {
            return Error.NotFound(
                "department.not.found",
                ["Department not found"],
                null);
        }

        bool existLocationsAndActive = await _locationRepository.ExistsAndActiveAsync(
            command.Request.LocationIds,
            cancellationToken);

        if (!existLocationsAndActive)
        {
            return Error.NotFound(
                "locations.not.found",
                ["Locations not found"],
                null);
        }

        var departmentLocations = command.Request.LocationIds
            .Select(locationId => new DepartmentLocation(command.DepartmentId, locationId));

        var transactionScopeResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using ITransactionScope transactionScope = transactionScopeResult.Value;

        await _departmentRepository.DeleteLocationsAsync(
            command.DepartmentId,
            cancellationToken);

        await _departmentRepository.AddLocationsAsync(
            departmentLocations,
            cancellationToken);

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

        return command.DepartmentId;
    }
}