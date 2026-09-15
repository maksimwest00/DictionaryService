using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentsByFilters;

public record GetDepartmentsByFiltersQuery(
    string? Search,
    string? SortBy,
    string? SortDir,
    int Page = 1,
    int PageSize = 20) : IQuery;