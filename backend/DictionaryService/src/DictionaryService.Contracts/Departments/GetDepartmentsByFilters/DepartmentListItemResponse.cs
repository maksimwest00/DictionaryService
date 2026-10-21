namespace DictionaryService.Contracts.Departments.GetDepartmentsByFilters;

public record DepartmentListItemResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Path { get; init; } = null!;

    public DateTime CreatedAt { get; init; }
}