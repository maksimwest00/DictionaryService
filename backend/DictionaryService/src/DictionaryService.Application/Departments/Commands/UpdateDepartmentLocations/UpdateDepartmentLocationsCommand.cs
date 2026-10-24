using DictionaryService.Contracts.Departments;
using Shared.Core.Abstractions;

namespace DictionaryService.Application.Departments.Commands.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest Request) : ICommand;