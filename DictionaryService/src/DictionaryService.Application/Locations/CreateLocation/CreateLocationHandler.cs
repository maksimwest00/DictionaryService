using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Domain.Locations;
using DictionaryService.Domain.Shared;
using FluentValidation;

namespace DictionaryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationHandler(
        ILocationRepository locationRepository,
        IValidator<CreateLocationCommand> validator)
    {
        _locationRepository = locationRepository;
        _validator = validator;
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

        return await _locationRepository.AddAsync(location, cancellationToken);
    }
}