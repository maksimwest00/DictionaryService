using Shared.Core.Abstractions;

namespace DictionaryService.Application.Locations.Commands.DeleteLocation;

public record DeleteLocationCommand(Guid Id) : ICommand;