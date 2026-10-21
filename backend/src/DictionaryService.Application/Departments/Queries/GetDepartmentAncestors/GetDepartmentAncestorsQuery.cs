using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentAncestors;

public record GetDepartmentAncestorsQuery(Guid Id) : IQuery;