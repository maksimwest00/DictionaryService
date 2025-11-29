namespace DictionaryService.Contracts.Departments;

public record GetDepartmentDto(
    string Search,
    int Page,
    int PageSize);