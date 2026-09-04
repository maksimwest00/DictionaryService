using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Positions.DeletePosition;

public record DeletePositionCommand(Guid Id) : ICommand;