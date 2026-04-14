using CSharpFunctionalExtensions;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Department?> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid[] departmentIds, CancellationToken cancellationToken);
}