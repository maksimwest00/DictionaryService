using DictionaryService.Domain.DepartmentLocations;

namespace DictionaryService.Domain.Locations;

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
        string timezone)
    {
        Id = Guid.NewGuid();
        Name = name;
        Address = address;
        if (string.IsNullOrEmpty(timezone))
        {
            throw new ArgumentException("Invalid timezone");
        }

        Timezone = timezone;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
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