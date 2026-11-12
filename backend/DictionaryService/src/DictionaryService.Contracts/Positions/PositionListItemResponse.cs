namespace DictionaryService.Contracts.Positions;

public record PositionListItemResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public bool IsActive { get; init; }
    public long DepartmentCount { get; init; }
}
