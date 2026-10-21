using Shared.Core.Validation;
using DictionaryService.Domain.Departments;
using FluentValidation;

namespace DictionaryService.Application.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Request.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(x => x.Request.LocationIds)
            .NotEmpty().WithMessage("LocationIds �� ����� ���� ������ ��������")
            .Must(HaveNoDuplicates).WithMessage("LocationIds �� ������ ��������� ����������");
    }

    private bool HaveNoDuplicates(Guid[] locationIds)
    {
        if (locationIds.Length == 0)
        {
            return true;
        }

        return locationIds.Length == locationIds.Distinct().Count();
    }
}