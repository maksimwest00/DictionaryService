using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.Queries;

public record GetDepartmentByIdQuery(Guid Id) : IQuery;