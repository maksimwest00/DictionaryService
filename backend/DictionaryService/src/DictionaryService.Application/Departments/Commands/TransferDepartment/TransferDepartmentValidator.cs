using FluentValidation;

namespace DictionaryService.Application.Departments.Commands.TransferDepartment;

public class TransferDepartmentValidator : AbstractValidator<TransferDepartmentCommand>
{
    public TransferDepartmentValidator()
    {
        RuleFor(x => x.Request.ParentId)
            .NotEqual(x => x.DepartmentId)
            .When(x => x.Request.ParentId.HasValue)
            .WithErrorCode("department.move.parent_is_self")
            .WithMessage("Department cannot be its own parent");
    }
}