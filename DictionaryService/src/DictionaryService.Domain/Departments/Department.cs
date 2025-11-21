namespace DictionaryService.Domain.Departments;

public class Department
{
    private readonly List<Department> _children;
    private readonly List<DepartmentLocation> _departmentLocations;
    private readonly List<DepartmentPosition> _departmentPositions;

    public Department(
        Name name,
        Identifier identifier,
        Guid? parentId,
        Path path,
        short depth,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        Department? parent,
        List<Department> children,
        IEnumerable<DepartmentLocation> departmentLocations,
        IEnumerable<DepartmentPosition> departmentPositions)
    {
        Id = Guid.NewGuid();
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Parent = parent;
        _children = children;
        _departmentLocations = departmentLocations.ToList();
        _departmentPositions = departmentPositions.ToList();
    }

    public Guid Id { get; private set; }

    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public Path Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> Locations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;
}