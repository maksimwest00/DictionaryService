using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts.Positions;

namespace DictionaryService.Application.Positions.RenamePosition;

public record RenamePositionCommand(Guid PositionId, RenamePositionRequest Request) : ICommand;