using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts.Departments;

namespace DictionaryService.Application.Departments.TransferDepartment;

public record TransferDepartmentCommand(Guid DepartmentId, TransferDepartmentRequest Request) : ICommand;
