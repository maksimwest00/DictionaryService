using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentById;

public record GetDepartmentByIdQuery(Guid Id) : IQuery;