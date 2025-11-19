namespace DictionaryService.Domain.Departments;

public class Position
{
    public Position(
        string name,
        string? description,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        List<Department> department)
    {
        Id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(name) ||
            (name.Length > 100 || name.Length < 3))
        {
            throw new ArgumentException("Name must be between 3 and 100 characters");
        }

        Name = name;

        if (description != null &&
            (description.Length > 1000 || description.Length < 3))
        {
            throw new ArgumentException("Description must be between 3 and 1000 characters");
        }

        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Department = department;
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<Department> Department { get; set; }
}
