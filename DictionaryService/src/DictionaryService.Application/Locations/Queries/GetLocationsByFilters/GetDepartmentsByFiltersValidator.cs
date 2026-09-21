using FluentValidation;

namespace DictionaryService.Application.Locations.Queries.GetLocationsByFilters;

public class GetLocationsByFiltersValidator : AbstractValidator<GetLocationsByFiltersQuery>
{
    public GetLocationsByFiltersValidator()
    {
        RuleFor(x => x.Search)
            .Must(x => x == null || x.Length <= 150);

        RuleFor(x => x.SortBy)
            .Must(x => x == null || new[] { "name", "created_at", "departmentCount" }.Contains(x));

        RuleFor(x => x.SortDir)
            .Must(x => x == null || new[] { "asc", "desc" }.Contains(x));

        RuleFor(x => x.Page)
            .Must(x => x >= 1);

        RuleFor(x => x.PageSize)
            .Must(x => x >= 1 && x <= 100);
    }
}