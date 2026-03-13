using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts.Locations;

namespace DictionaryService.Application.Locations.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;