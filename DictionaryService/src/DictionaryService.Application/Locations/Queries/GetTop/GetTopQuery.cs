using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Locations.Queries.GetTop;

public record GetTopQuery(Guid Id) : IQuery;