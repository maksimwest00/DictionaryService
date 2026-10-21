using Shared.Core.Abstractions;
using DictionaryService.Contracts.Departments;

namespace DictionaryService.Application.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;