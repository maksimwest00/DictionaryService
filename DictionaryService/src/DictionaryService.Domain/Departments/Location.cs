namespace DictionaryService.Domain.Departments;

public class Location
{
    public Location(
        string name,
        string address,
        string timezone,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        List<Department> departments)
    {
        Id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(name) ||
            (name.Length > 120 || name.Length < 3))
        {
            throw new ArgumentException("Name must be between 3 and 150 characters");
        }

        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Departments = departments;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Address { get; private set; }

    public string Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public List<Department> Departments { get; private set; }
}