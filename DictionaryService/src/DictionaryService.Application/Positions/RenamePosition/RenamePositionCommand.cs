using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts.Positions;

namespace DictionaryService.Application.Positions.RenamePosition;

public record RenamePositionCommand(RenamePositionRequest Request) : ICommand;