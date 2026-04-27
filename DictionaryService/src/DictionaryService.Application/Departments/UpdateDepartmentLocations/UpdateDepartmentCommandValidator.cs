using FluentValidation;

namespace DictionaryService.Application.Departments.UpdateDepartmentLocations;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentCommandValidator()
    {
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