using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Application.Locations;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Domain.DepartmentLocations;
using DictionaryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace DictionaryService.Application.Departments.UpdateDepartmentLocations;

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
        ValidationResult? validationResult = await _validator.ValidateAsync(command, cancellationToken);

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
            return Result.Failure<Guid, Error>(Error.Failure(null, ["Department not found"]));
        }

        bool existLocationsAndActive =
            await _locationRepository.ExistsAndActiveAsync(
                command.Request.LocationIds,
                cancellationToken);

        if (!existLocationsAndActive)
        {
            return Result.Failure<Guid, Error>(Error.Failure(null, ["Locations not found"]));
        }

        IEnumerable<DepartmentLocation> departmentLocations = command.Request.LocationIds
            .Select(locationId => new DepartmentLocation(command.DepartmentId, locationId));

        await _departmentRepository.DeleteLocationsAsync(
            command.DepartmentId,
            cancellationToken);

        await _departmentRepository.AddLocationsAsync(
            departmentLocations,
            cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        return command.DepartmentId;
    }
}