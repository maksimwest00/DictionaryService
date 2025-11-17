namespace DictionaryService.Domain.Departments;

public class Location
{
    public Guid Id { get; set; }
    // TODO UNIQUE, 3–120 симв.
    public string Name { get; set; }
    // TODO string/json В бд может быть несколько столбцов или jsonb
    public string Address { get; set; }
    public string Timezone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Department> Departments { get; set; } = [];
}