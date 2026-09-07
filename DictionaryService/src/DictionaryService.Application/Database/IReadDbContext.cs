using DictionaryService.Domain.Locations;

namespace DictionaryService.Application.Database;

public interface IReadDbContext
{
    IQueryable<Location> LocationsRead { get; }
}