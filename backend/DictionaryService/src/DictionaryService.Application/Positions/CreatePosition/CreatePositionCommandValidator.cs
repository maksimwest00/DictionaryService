using DictionaryService.Domain.Positions;
using FluentValidation;
using Shared.Core.Validation;

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
            .NotEmpty().WithMessage("DepartmentIds �� ����� ���� ������ ��������")
            .Must(HaveNoDuplicates).WithMessage("DepartmentIds �� ������ ��������� ����������");
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