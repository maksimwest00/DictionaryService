using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts.Departments;

namespace DictionaryService.Application.Departments.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest Request) : ICommand;