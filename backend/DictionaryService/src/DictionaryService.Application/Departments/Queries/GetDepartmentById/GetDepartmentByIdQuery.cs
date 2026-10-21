using Shared.Core.Abstractions;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentById;

public record GetDepartmentByIdQuery(Guid Id) : IQuery;