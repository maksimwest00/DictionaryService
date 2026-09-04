using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Positions.DeletePosition;

public record DeletePositionCommand : ICommand
{
    public Guid Id { get; }

    public DeletePositionCommand(Guid id)
    {
        Id = id;
    }
}