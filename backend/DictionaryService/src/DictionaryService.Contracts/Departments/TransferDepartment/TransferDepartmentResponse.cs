namespace DictionaryService.Contracts.Departments.TransferDepartment;

public record TransferDepartmentResponse(
    Guid Id,
    Guid? ParentId,
    string Path,
    int Depth,
    DateTime UpdatedAt);