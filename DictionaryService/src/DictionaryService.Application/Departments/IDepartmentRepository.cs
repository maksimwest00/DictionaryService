using CSharpFunctionalExtensions;
using DictionaryService.Domain.DepartmentLocations;
using DictionaryService.Domain.DepartmentPositions;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid[] departmentIds, CancellationToken cancellationToken);

    Task<bool> ExistsAndActiveAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteLocationsAsync(
        Guid departmentId,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> AddLocationsAsync(
        IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> IsDepartmentContains(
        Department department,
        Department newParent,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> TransferAsync(
        Department department,
        Department? newParent,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> AddPositionAsync(
        DepartmentPosition departmentPosition,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeletePositionAsync(
        DepartmentPosition departmentPosition,
        CancellationToken cancellationToken);
}