namespace DictionaryService.Contracts;

public record GetDepartmentDto(
    string Search,
    int Page,
    int PageSize);