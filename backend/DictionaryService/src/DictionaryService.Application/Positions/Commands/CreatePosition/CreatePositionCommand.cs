using DictionaryService.Contracts.Positions;
using Shared.Core.Abstractions;

namespace DictionaryService.Application.Positions.Commands.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;