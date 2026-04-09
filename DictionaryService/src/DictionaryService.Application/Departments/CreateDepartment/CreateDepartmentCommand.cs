using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts.Departments;

namespace DictionaryService.Application.Departments.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;