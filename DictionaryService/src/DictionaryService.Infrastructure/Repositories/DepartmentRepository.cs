using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Departments;
using DictionaryService.Domain.DepartmentLocations;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Path = DictionaryService.Domain.Departments.Path;

namespace DictionaryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly DictionaryServiceDbContext _dbContext;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(
        ILogger<DepartmentRepository> logger,
        DictionaryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(
        Department department,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return department.Id;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName == "ix_departments_identifier")
            {
                _logger.LogWarning(
                    "Попытка создать отдел с уже существующим identifier (DepartmentId={DepartmentId}, Identifier={Identifier})",
                    department.Id,
                    department.Identifier.Value);

                return Result.Failure<Guid, Error>(Error.Conflict(
                    null,
                    ["A department with the same identifier already exists"]));
            }

            _logger.LogError(
                ex,
                "Ошибка при сохранении отдела в БД (DepartmentId={DepartmentId})",
                department.Id);

            return Result.Failure<Guid, Error>(Error.Conflict(
                null,
                ["An error occurred while saving the department to the database"]));
        }
    }

    public async Task<Department?> GetByIdAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Departments.FirstOrDefaultAsync(
            d => d.Id == departmentId && d.IsActive,
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid[] departmentIds,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Departments
            .AnyAsync(l => departmentIds.Contains(l.Id), cancellationToken);
    }

    public async Task<bool> ExistsAndActiveAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Departments
            .AnyAsync(l => l.Id == departmentId && l.IsActive, cancellationToken);
    }

    public async Task<UnitResult<Error>> DeleteLocationsAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        await _dbContext.DepartmentLocations
            .Where(d => d.DepartmentId == departmentId)
            .ExecuteDeleteAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> AddLocationsAsync(
        IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken)
    {
        await _dbContext.DepartmentLocations
            .AddRangeAsync(departmentLocations, cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> SaveUpdateLocationsAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Ошибка при обновлении локаций отдела в БД (DepartmentId={DepartmentId})",
                departmentId);

            return Result.Failure<Guid, Error>(Error.Conflict(
                null,
                ["An error occurred while updating locations department to the database"]));
        }
    }

    public async Task<UnitResult<Error>> IsDepartmentContains(
        Department department,
        Department newParent,
        CancellationToken cancellationToken)
    {
        Path path = department.Path;

        string query = """
                       SELECT COUNT(*)
                       FROM departments
                       WHERE path @> @path::ltree
                           AND path != @path::ltree
                           AND id = @newParentId
                       """;

        var connection = _dbContext.Database.GetDbConnection();

        int count = await connection.ExecuteScalarAsync<int>(
            query,
            new
            {
                path = path.Value,
                newParentId = newParent.Id,
            });

        if (count == 0)
        {
            return UnitResult.Success<Error>();
        }
        else
        {
            return UnitResult.Failure<Error>(Error.NotFound(
                null,
                ["New parent department include in department"],
                department.Id));
        }
    }

    public async Task<UnitResult<Error>> TransferAsync(
        Department department,
        Department? newParent,
        CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();

        string query = """
                       UPDATE departments
                       SET path = @newPath::ltree || subpath(path, depth),
                           parent_id = @newParentId,
                           depth = nlevel(@newPath::ltree)
                       WHERE path = @oldPath::ltree;
                       
                       UPDATE departments
                       SET path = @newPath::ltree || subpath(path, depth - 1),
                           depth = nlevel(@newPath::ltree) + (nlevel(@oldPath::ltree) - 1)
                       WHERE path <@ @oldPath::ltree
                         AND path != @oldPath::ltree;
                       """;

        await connection.ExecuteAsync(
            query,
            param: new
            {
                id = department.Id,
                oldPath = department.Path.Value,
                newPath = newParent?.Path.Value,
                newParentId = newParent?.Id,
            });

        return UnitResult.Success<Error>();
    }
}