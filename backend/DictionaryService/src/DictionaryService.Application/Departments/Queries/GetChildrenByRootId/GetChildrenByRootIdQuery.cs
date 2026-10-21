using Shared.Core.Abstractions;

namespace DictionaryService.Application.Departments.Queries.GetChildrenByRootId;

public record GetChildrenByRootIdQuery(Guid Id) : IQuery;