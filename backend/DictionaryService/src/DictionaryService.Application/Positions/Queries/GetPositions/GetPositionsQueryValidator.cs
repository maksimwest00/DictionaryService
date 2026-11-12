using FluentValidation;

namespace DictionaryService.Application.Positions.Queries.GetPositions;

public class GetPositionsQueryValidator : AbstractValidator<GetPositionsQuery>
{
    public GetPositionsQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize должен быть от 1 до 100");
    }
}
