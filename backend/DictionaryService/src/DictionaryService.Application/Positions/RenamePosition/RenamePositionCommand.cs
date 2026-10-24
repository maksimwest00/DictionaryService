using DictionaryService.Contracts.Positions;
using Shared.Core.Abstractions;

namespace DictionaryService.Application.Positions.RenamePosition;

public record RenamePositionCommand(Guid PositionId, RenamePositionRequest Request) : ICommand;