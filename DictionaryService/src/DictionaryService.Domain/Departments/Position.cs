namespace DictionaryService.Domain.Departments;

public class Position
{
    private readonly List<DepartmentPosition> _departmentPositions = [];

    // EF core
    private Position()
    {
    }

    public Position(
        Name name,
        Description description,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        List<DepartmentPosition> departmentPositions)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        _departmentPositions = departmentPositions;
    }

    public Guid Id { get; private set; }

    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public List<DepartmentPosition> DepartmentPositions => _departmentPositions;
}