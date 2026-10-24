using DictionaryService.Contracts.Locations.CreateLocation;
using Shared.Core.Abstractions;

namespace DictionaryService.Application.Locations.Commands.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;