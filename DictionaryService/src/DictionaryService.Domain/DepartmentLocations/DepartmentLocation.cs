using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Locations;

namespace DictionaryService.Domain.DepartmentLocations;

public class DepartmentLocation
{
    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid LocationId { get; private set; }

    public Department Department { get; private set; } = null!;

    public Location Location { get; private set; } = null!;
}