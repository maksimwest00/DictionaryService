namespace DictionaryService.Domain.Departments;

public class Location
{
    private readonly List<DepartmentLocation> _departmentLocations = [];

    // EF core
    private Location()
    {
    }

    public Location(
        Name name,
        Address address,
        string timezone,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        List<DepartmentLocation> departmentLocations)
    {
        Id = Guid.NewGuid();
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        _departmentLocations = departmentLocations;
    }

    public Guid Id { get; private set; }

    public Name Name { get; private set; }

    public Address Address { get; private set; }

    public string Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;
}