using DictionaryService.Application.Validation;
using DictionaryService.Domain.Positions;
using FluentValidation;

namespace DictionaryService.Application.Positions.CreatePosition;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Request.Description)
            .MustBeValueObject(Description.Create);

        RuleFor(x => x.Request.DepartmentIds)
            .NotEmpty().WithMessage("DepartmentIds не может быть пустым массивом")
            .Must(HaveNoDuplicates).WithMessage("DepartmentIds не должен содержать дубликатов");
    }

    private bool HaveNoDuplicates(Guid[] locationIds)
    {
        if (locationIds.Length == 0)
        {
            return false;
        }

        return locationIds.Length == locationIds.Distinct().Count();
    }
}