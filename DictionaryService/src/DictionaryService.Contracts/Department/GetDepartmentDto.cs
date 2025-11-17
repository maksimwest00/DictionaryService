namespace DictionaryService.Contracts.Department;

public record GetDepartmentDto(
    string Search,
    int Page,
    int PageSize);