using System.Text.RegularExpressions;

namespace DictionaryService.Domain.Departments;

public class Department
{
    private readonly List<Department> _children;
    private readonly List<Location> _locations;
    private readonly List<Position> _positions;

    public Department(
        string name,
        string identifier,
        Guid? parentId,
        string path,
        short depth,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        Department? parent,
        List<Department> children,
        IEnumerable<Location> locations,
        IEnumerable<Position> positions)
    {
        Id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(name) ||
            (name.Length > 150 || name.Length < 3))
        {
            throw new ArgumentException("Name must be between 3 and 150 characters");
        }

        Name = name;

        if (string.IsNullOrWhiteSpace(identifier) ||
            (identifier.Length > 150 || identifier.Length < 3))
        {
            throw new ArgumentException("Identifier must be between 3 and 150 characters");
        }

        if (!Regex.IsMatch(identifier, @"^[a-zA-Z]*$"))
        {
            throw new ArgumentException("Identifier must be in Latin characters");
        }

        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Parent = parent;
        _children = children;
        _locations = locations.ToList();
        _positions = positions.ToList();
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<Location> Locations => _locations;

    public IReadOnlyList<Position> Positions => _positions;
}