using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Locations.Queries.GetLocationsByFilters;

public record GetLocationsByFiltersQuery(
    string? Search,
    int? MinDepartmentCount,
    string? SortBy = "name",
    string? SortDir = "asc",
    int Page = 1,
    int PageSize = 20) : IQuery;