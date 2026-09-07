using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts.Departments;

namespace DictionaryService.Application.Departments.Commands.TransferDepartment;

public record TransferDepartmentCommand(Guid DepartmentId, TransferDepartmentRequest Request) : ICommand;
