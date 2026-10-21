using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts.Departments;

namespace DictionaryService.Application.Departments.Commands.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest Request) : ICommand;