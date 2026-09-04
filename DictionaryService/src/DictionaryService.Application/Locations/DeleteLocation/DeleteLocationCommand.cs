using DictionaryService.Application.Abstractions;

namespace DictionaryService.Application.Locations.DeleteLocation;

public record DeleteLocationCommand(Guid Id) : ICommand;