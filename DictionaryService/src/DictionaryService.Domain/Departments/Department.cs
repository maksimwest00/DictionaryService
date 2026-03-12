using CSharpFunctionalExtensions;
using DictionaryService.Domain.DepartmentLocations;
using DictionaryService.Domain.DepartmentPositions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Departments;

public sealed class Department
{
    private readonly List<Department> _children;
    private readonly List<DepartmentLocation> _departmentLocations;
    private readonly List<DepartmentPosition> _departmentPositions;

    // EF Core
    private Department()
    {
    }

    public Department(
        Guid id,
        Name name,
        Identifier identifier,
        Guid? parentId,
        Path path,
        int depth,
        Department? parent,
        List<Department> children,
        IEnumerable<DepartmentLocation> departmentLocations,
        IEnumerable<DepartmentPosition> departmentPositions)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        ChildrenCount = Children.Count;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
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

    public int Depth { get; private set; }

    public int ChildrenCount { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public static Result<Department, Error> CreateParent(
        Name name,
        Identifier identifier,
        IEnumerable<DepartmentLocation> departmentLocations,
        Guid? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return Error.Validation("department.location", ["Department locations must contain at least one location"]);
        }

        var path = Path.CreateParent(identifier);
        return new Department(
            departmentId ?? Guid.NewGuid(),
            name,
            identifier,
            null,
            path,
            0,
            null,
            [],
            departmentLocationsList,
            []);
    }

    public static Result<Department, Error> CreateChild(
        Name name,
        Identifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation> departmentLocations,
        Guid? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return Error.Validation("department.location", ["Department locations must contain at least one location"]);
        }

        var path = parent.Path.CreateChild(identifier);

        return new Department(
            departmentId ?? Guid.NewGuid(),
            name,
            identifier,
            parent.Id,
            path,
            parent.Depth + 1,
            parent,
            [],
            departmentLocationsList,
            []);
    }
}