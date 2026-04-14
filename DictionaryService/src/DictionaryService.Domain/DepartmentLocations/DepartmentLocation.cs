using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Locations;

namespace DictionaryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid LocationId { get; private set; }

    public DepartmentLocation(Guid departmentId, Guid locationId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}