namespace DictionaryService.Domain.Departments;

public class Location
{
    public Location(
        Name name,
        Address address,
        string timezone,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        List<DepartmentLocation> departments)
    {
        Id = Guid.NewGuid();
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Departments = departments;
    }

    public Guid Id { get; private set; }

    public Name Name { get; private set; }

    public Address Address { get; private set; }

    public string Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public List<DepartmentLocation> Departments { get; private set; }
}