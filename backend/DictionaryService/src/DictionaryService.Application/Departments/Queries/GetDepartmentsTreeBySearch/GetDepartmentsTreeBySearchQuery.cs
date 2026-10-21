using Shared.Core.Abstractions;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentsTreeBySearch;

public record GetDepartmentsTreeBySearchQuery(string search) : IQuery;