using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Domain.Locations;
using DictionaryService.Domain.Shared;
using FluentValidation;

namespace DictionaryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public CreateLocationHandler(
        ILocationRepository locationRepository,
        IValidator<CreateLocationCommand> validator,
        ITransactionManager transactionManager)
    {
        _locationRepository = locationRepository;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> HandleAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var nameLocationResult = Name.Create(command.Request.Name);

        var addressLocationResult = Address.Create(
            command.Request.Address.City,
            command.Request.Address.Street,
            command.Request.Address.Building,
            command.Request.Address.RoomNumber);

        Location location = new(
            nameLocationResult.Value,
            addressLocationResult.Value,
            command.Request.Timezone);

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using var transactionScope = transactionScopeResult.Value;

        var locationId = await _locationRepository.AddAsync(location, cancellationToken);

        var commitedResult = transactionScope.Commit();

        if (commitedResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitedResult.Error;
        }

        return locationId;
    }
}