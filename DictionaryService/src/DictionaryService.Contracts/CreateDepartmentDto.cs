namespace DictionaryService.Contracts;

public record CreateDepartmentDto(
    string Name,
    string Identifier,
    Guid? ParentId,
    string Path,
    short Depth,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid? ChildId,
    Guid[] LocationIds,
    Guid[] PositionIds
);