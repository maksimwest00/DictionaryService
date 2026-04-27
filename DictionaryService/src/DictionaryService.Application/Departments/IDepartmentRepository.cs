using CSharpFunctionalExtensions;
using DictionaryService.Domain.DepartmentLocations;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Department?> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid[] departmentIds, CancellationToken cancellationToken);

    Task<bool> ExistsAndActiveAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> SaveUpdateLocationsAsync(
        Guid departmentId,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteLocationsAsync(
        Guid departmentId,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> AddLocationsAsync(
        IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken);
}