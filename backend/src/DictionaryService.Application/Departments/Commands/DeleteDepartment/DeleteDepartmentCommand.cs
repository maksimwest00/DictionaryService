using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;