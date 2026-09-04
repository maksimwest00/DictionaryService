using CSharpFunctionalExtensions;
using DictionaryService.Application.Locations;
using DictionaryService.Domain.Locations;
using DictionaryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly DictionaryServiceDbContext _dbContext;

    public LocationRepository(DictionaryServiceDbContext dbContext) => _dbContext = dbContext;

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
        CancellationToken cancellationToken) =>
        await _dbContext.Locations
            .AnyAsync(l => locationIds.Contains(l.Id), cancellationToken);

    public async Task<bool> ExistsAndActiveAsync(
        Guid[] locationIds,
        CancellationToken cancellationToken) =>
        await _dbContext.Locations
            .AnyAsync(l => locationIds.Contains(l.Id) && l.IsActive, cancellationToken);

    public async Task<Result<Location, Error>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Location? location = await _dbContext.Locations
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

        if (location is null)
        {
            return Error.NotFound(null, ["Location not found"], id);
        }

        return location;
    }
}