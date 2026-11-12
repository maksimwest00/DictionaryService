using DictionaryService.Domain.Positions;
using FluentValidation;
using Shared.Core.Validation;

namespace DictionaryService.Application.Positions.Commands.RenamePosition;

public class RenamePositionCommandValidator : AbstractValidator<RenamePositionCommand>
{
    public RenamePositionCommandValidator()
    {
        RuleFor(r => r.Request.Name)
            .MustBeValueObject(Name.Create);
    }
}