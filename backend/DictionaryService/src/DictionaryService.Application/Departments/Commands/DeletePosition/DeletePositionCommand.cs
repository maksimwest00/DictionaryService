using Shared.Core.Abstractions;

namespace DictionaryService.Application.Departments.Commands.DeletePosition;

public record DeletePositionCommand(Guid DeptId, Guid PosId) : ICommand;
