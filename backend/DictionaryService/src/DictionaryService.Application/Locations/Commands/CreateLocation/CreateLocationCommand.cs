using Shared.Core.Abstractions;
using DictionaryService.Contracts.Locations.CreateLocation;

namespace DictionaryService.Application.Locations.Commands.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;