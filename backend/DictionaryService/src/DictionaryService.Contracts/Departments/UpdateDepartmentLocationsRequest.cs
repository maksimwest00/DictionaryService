namespace DictionaryService.Contracts.Departments;

public record UpdateDepartmentLocationsRequest(
    Guid[] LocationIds
);