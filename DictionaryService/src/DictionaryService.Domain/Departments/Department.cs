namespace DictionaryService.Domain.Departments;

public class Department
{
    public Guid Id { get; set; }
    // TODO 3–150 символов, NOT NULL
    public string Name { get; set; } = null!;
    // TODO 3–150 символов, NOT NULL, только латиница
    public string Identifier { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public string Path { get; set; } = string.Empty;
    public short Depth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Department? Parent { get; set; }
    
    public Department? ChildDepartment { get; set; }
    public List<Location> Locations { get; set; } = [];
    public List<Position> Positions { get; set; } = [];
}