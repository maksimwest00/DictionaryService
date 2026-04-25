namespace DictionaryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    public DepartmentLocation(
        Guid departmentId,
        Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid LocationId { get; private set; }
}