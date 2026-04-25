using DictionaryService.Application.Locations;
using DictionaryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> ExistsAsync(
        Guid[] locationIds,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Locations
            .AnyAsync(l => locationIds.Contains(l.Id), cancellationToken);
    }

    public async Task<bool> ExistsAndActiveAsync(
        Guid[] locationIds,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Locations
            .AnyAsync(l => locationIds.Contains(l.Id) && l.IsActive, cancellationToken);
    }
}