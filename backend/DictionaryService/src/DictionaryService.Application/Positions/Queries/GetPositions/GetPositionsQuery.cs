using Shared.Core.Abstractions;

namespace DictionaryService.Application.Positions.Queries.GetPositions;

public record GetPositionsQuery(
    string? Cursor,
    int PageSize = 20,
    Guid? DepartmentId = null) : IQuery;
