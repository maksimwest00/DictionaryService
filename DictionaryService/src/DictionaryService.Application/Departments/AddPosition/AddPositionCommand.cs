using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Departments.AddPosition;

public record AddPositionCommand(Guid DeptId, Guid PosId) : ICommand;