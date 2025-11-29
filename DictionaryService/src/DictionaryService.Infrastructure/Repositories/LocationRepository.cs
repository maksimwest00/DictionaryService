using DictionaryService.Application.Locations;
using DictionaryService.Domain.Departments;

namespace DictionaryService.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly DictionaryServiceDbContext _dbContext;

    public LocationRepository(DictionaryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(
        Location location,
        CancellationToken cancellationToken)
    {
        await _dbContext.AddAsync(location, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return location.Id;
    }
}