using DictionaryService.Contracts.Departments;
using Shared.Core.Abstractions;

namespace DictionaryService.Application.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;