using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;