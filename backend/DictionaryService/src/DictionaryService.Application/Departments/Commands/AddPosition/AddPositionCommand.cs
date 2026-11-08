using Shared.Core.Abstractions;

namespace DictionaryService.Application.Departments.Commands.AddPosition;

public record AddPositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;