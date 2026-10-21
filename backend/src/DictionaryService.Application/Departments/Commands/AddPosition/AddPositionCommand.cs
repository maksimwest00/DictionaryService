using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.Commands.AddPosition;

public record AddPositionCommand(Guid DeptId, Guid PosId) : ICommand;