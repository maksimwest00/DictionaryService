using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Departments;
using DictionaryService.Contracts.Departments.TransferDepartment;
using DictionaryService.Domain.DepartmentLocations;
using DictionaryService.Domain.DepartmentPositions;
using DictionaryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Kernel;
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

    public async Task<Result<Department, Error>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"""
                 SELECT *
                 FROM departments
                 WHERE id = {id}
                   AND is_active = true
                 FOR UPDATE;
             """,
            cancellationToken);

        Department? department = await _dbContext.Departments
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

        if (department is null)
        {
            return Error.NotFound("department.not_found", ["Department not found"], id);
        }

        return department;
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

    public async Task<UnitResult<Error>> IsDepartmentContains(
        Department department,
        Department newParent,
        CancellationToken cancellationToken)
    {
        Path path = department.Path;

        const string query = """
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

    public async Task<bool> CheckCycleAsync(
        Guid departmentId,
        Guid newParentId,
        CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();

        // Проверка по parent_id для случая, когда новая родительская связь уже установлена другой транзакцией
        var parentCheck = await connection.ExecuteScalarAsync<int>(
            """
                WITH RECURSIVE descendants AS (
                    SELECT id, parent_id
                    FROM departments
                    WHERE id = @departmentId
                    UNION ALL
                    SELECT d.id, d.parent_id
                    FROM departments d
                    INNER JOIN descendants ds ON d.parent_id = ds.id
                )
                SELECT 1
                FROM descendants
                WHERE id = @newParentId
            """,
            new
            {
                departmentId = departmentId,
                newParentId = newParentId,
            });

        if (parentCheck == 1)
        {
            return true;
        }

        // Проверка по path для случая, когда path уже обновлен
        var cond = await connection.ExecuteScalarAsync<int>(
            """
                -- получаем путь двигаемого департамента
                WITH parent AS (
                    SELECT path
                    FROM departments
                    WHERE id = @departmentId
                ),
                -- получаем список его детей
                childrens AS (
                    SELECT *
                    FROM departments
                    WHERE path::ltree <@ (SELECT path FROM parent)
                      AND path::ltree <> (SELECT path FROM parent)
                )

                -- смотрим есть ли среди детей тот департамент в который мы перемещаем
                SELECT 1
                FROM childrens
                WHERE id = @parentId
            """,
            new
            {
                departmentId = departmentId,
                parentId = newParentId,
            });

        return cond == 1;
    }

    public async Task<UnitResult<Error>> AddPositionAsync(
        DepartmentPosition departmentPosition,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.DepartmentPositions.AnyAsync(
                x =>
                    x.DepartmentId == departmentPosition.DepartmentId &&
                    x.PositionId == departmentPosition.PositionId, cancellationToken))
        {
            return Error.Conflict(null, ["This record exist"]);
        }

        await _dbContext.DepartmentPositions
            .AddAsync(departmentPosition, cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> AddPositionsAsync(
        IEnumerable<DepartmentPosition> departmentPositions,
        CancellationToken cancellationToken)
    {
        var departmentPositionsList = departmentPositions.ToList();

        // Check each pair individually
        foreach (var dp in departmentPositionsList)
        {
            var exists = await _dbContext.DepartmentPositions
                .AnyAsync(
                    x =>
                    x.DepartmentId == dp.DepartmentId &&
                    x.PositionId == dp.PositionId, cancellationToken);

            if (exists)
            {
                return Error.Conflict(null, ["This record exist"]);
            }
        }

        await _dbContext.DepartmentPositions
            .AddRangeAsync(departmentPositionsList, cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> DeletePositionAsync(
        DepartmentPosition departmentPosition,
        CancellationToken cancellationToken)
    {
        var dPosition = await _dbContext.DepartmentPositions.FirstOrDefaultAsync(
            x =>
                x.DepartmentId == departmentPosition.DepartmentId &&
                x.PositionId == departmentPosition.PositionId, cancellationToken);

        if (dPosition is null)
        {
            return Error.NotFound(null, ["This record is not exist"], null);
        }

        _dbContext.DepartmentPositions.Remove(dPosition);

        return UnitResult.Success<Error>();
    }

    public async Task<Result<TransferDepartmentResponse, Error>> TransferAsync(
        Department department,
        Department? newParent,
        CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();

        try
        {
            var parameters = new
            {
                id = department.Id,
                oldPath = department.Path.Value,
                newPath = newParent?.Path.Value,
                newParentId = newParent?.Id,
            };

            int rowsAffected;
            if (newParent is not null)
            {
                rowsAffected = await connection.ExecuteAsync(
                    """
                    UPDATE departments
                    SET path = @newPath::ltree || subpath(path, depth),
                        parent_id = @newParentId,
                        depth = nlevel(@newPath::ltree)
                    WHERE id = @id AND path = @oldPath::ltree;

                    UPDATE departments
                    SET path = @newPath::ltree || subpath(path, depth - 1),
                        depth = nlevel(@newPath::ltree) + (nlevel(@oldPath::ltree) - 1)
                    WHERE path <@ @oldPath::ltree
                      AND path != @oldPath::ltree;
                    """,
                    parameters);
            }
            else
            {
                rowsAffected = await connection.ExecuteAsync(
                    """
                    UPDATE departments
                    SET path = subpath(path, nlevel(path) - 1),
                        parent_id = NULL,
                        depth = 0
                    WHERE id = @id AND path = @oldPath::ltree;

                    UPDATE departments
                    SET path = subpath(@oldPath::ltree, nlevel(@oldPath::ltree) - 1) || subpath(path, nlevel(@oldPath::ltree)),
                        depth = nlevel(path) - nlevel(@oldPath::ltree) + 1
                    WHERE path <@ @oldPath::ltree
                      AND path != @oldPath::ltree;
                    """,
                    parameters);
            }

            if (rowsAffected == 0)
            {
                return Result.Failure<TransferDepartmentResponse, Error>(
                    Error.Conflict(
                        "department.move.conflict",
                        ["Department was modified by another transaction"]));
            }
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DeadlockDetected || ex.SqlState == "40001")
        {
            _logger.LogWarning(
                ex,
                "Transaction conflict detected при перемещении отдела (DepartmentId={DepartmentId}, SqlState={SqlState})",
                department.Id,
                ex.SqlState);

            return Result.Failure<TransferDepartmentResponse, Error>(
                Error.Conflict(
                    "department.move.conflict",
                    ["Transaction conflict occurred due to concurrent operations. Please retry."]));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка при перемещении отдела в БД (DepartmentId={DepartmentId})",
                department.Id);

            return Result.Failure<TransferDepartmentResponse, Error>(
                Error.Conflict(
                    null,
                    ["An error occurred while transfer the department to the database"]));
        }

        var departmentEntity = await connection.QuerySingleAsync(
            """
                    SELECT id, parent_id, path, depth, updated_at
                    FROM departments
                    WHERE id = @id
                """,
            new { id = department.Id });

        return new TransferDepartmentResponse(
            departmentEntity.id,
            departmentEntity.parent_id,
            departmentEntity.path,
            departmentEntity.depth,
            departmentEntity.updated_at);
    }
}