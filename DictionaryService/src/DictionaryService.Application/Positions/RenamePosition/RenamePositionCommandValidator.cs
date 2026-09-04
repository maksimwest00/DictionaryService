using DictionaryService.Application.Validation;
using DictionaryService.Domain.Positions;
using FluentValidation;

namespace DictionaryService.Application.Positions.RenamePosition;

public class RenamePositionCommandValidator : AbstractValidator<RenamePositionCommand>
{
    public RenamePositionCommandValidator()
    {
        RuleFor(r => r.Request.Name)
            .MustBeValueObject(Name.Create);
    }
}