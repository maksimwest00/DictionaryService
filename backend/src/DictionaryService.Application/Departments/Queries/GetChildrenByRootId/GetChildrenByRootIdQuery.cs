using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.Queries.GetChildrenByRootId;

public record GetChildrenByRootIdQuery(Guid Id) : IQuery;