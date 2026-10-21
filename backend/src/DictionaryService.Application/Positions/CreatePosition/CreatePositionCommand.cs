using DictionaryService.Application.Abstractions;
using DictionaryService.Contracts;
using DictionaryService.Contracts.Positions;

namespace DictionaryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;