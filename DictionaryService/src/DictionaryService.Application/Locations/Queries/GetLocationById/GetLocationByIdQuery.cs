using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Locations.Queries.GetLocationById;

public record GetLocationByIdQuery(Guid Id) : IQuery;