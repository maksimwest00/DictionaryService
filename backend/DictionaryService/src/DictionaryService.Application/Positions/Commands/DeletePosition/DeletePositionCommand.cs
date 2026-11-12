using Shared.Core.Abstractions;

namespace DictionaryService.Application.Positions.Commands.DeletePosition;

public record DeletePositionCommand(Guid Id) : ICommand;