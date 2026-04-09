using DictionaryService.Application.Validation;
using DictionaryService.Domain.Departments;
using FluentValidation;

namespace DictionaryService.Application.Departments.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Request.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(x => x.Request.LocationIds)
            .NotEmpty().WithMessage("LocationIds не может быть пустым массивом")
            .Must(HaveNoDuplicates).WithMessage("LocationIds не должен содержать дубликатов");
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