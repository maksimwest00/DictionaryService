namespace DictionaryService.Domain.Departments;

public class Position
{
    public Guid Id { get; set; }
    // TODO UNIQUE, 3–100 симв.
    public string Name { get; set; }
    // TODO ≤ 1000 симв.
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Department> Department { get; set; }
}
