using DictionaryService.Domain.DepartmentPositions;

namespace DictionaryService.Domain.Positions;

public class Position
{
    private readonly List<DepartmentPosition> _departmentPositions;

    // EF core
    private Position()
    {
    }

    public Position(
        Name name,
        Description description,
        IEnumerable<Guid> departmentIds)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _departmentPositions = departmentIds.Select(departmentId =>
                new DepartmentPosition(departmentId, Id))
            .ToList();
    }

    public Guid Id { get; }

    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public static Position Create(
        Name name,
        Description description,
        IEnumerable<Guid> departmentIds)
    {
        return new Position(name, description, departmentIds);
    }

    public void UpdateName(Name name)
    {
        Name = name;
    }
}