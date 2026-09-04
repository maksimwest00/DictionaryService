using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.DeletePosition;

public record DeletePositionCommand(Guid DeptId, Guid PosId) : ICommand;
