using Shared.Core.Validation;
using DictionaryService.Domain.Locations;
using Shared.Kernel;
using FluentValidation;

namespace DictionaryService.Application.Locations.Commands.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Request.Address)
            .MustBeValueObject(x =>
                Address.Create(
                    x.City,
                    x.Street,
                    x.Building,
                    x.RoomNumber));

        RuleFor(x => x.Request.Timezone)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("Timezone", "value is empty"));
    }
}