namespace DictionaryService.Contracts.Department;

public record UpdateDepartmentDto(
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