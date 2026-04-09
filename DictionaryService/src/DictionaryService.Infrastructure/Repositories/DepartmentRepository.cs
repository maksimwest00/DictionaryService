using CSharpFunctionalExtensions;
using DictionaryService.Application.Departments;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

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
            d => d.Id == departmentId,
            cancellationToken);
    }
}