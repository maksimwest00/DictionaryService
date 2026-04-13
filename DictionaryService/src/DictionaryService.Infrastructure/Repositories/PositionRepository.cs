using CSharpFunctionalExtensions;
using DictionaryService.Application.Positions;
using DictionaryService.Domain.Positions;
using DictionaryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DictionaryService.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly ILogger<PositionRepository> _logger;
    private readonly DictionaryServiceDbContext _dbContext;

    public PositionRepository(
        ILogger<PositionRepository> logger,
        DictionaryServiceDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Error>> AddAsync(
        Position position,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Positions.AddAsync(position, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return position.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка при сохранении должности в БД (PositionId={PositionId})",
                position.Id);

            return Result.Failure<Guid, Error>(Error.Conflict(
                null,
                ["An error occurred while saving the department to the database"]));
        }
    }

    public async Task<bool> IsExistPositionNameAsync(
        string positionName,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Positions
            .AnyAsync(x => x.Name.Value == positionName && x.IsActive, cancellationToken);
    }
}