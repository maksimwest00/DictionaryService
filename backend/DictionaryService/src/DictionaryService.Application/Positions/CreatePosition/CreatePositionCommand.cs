using DictionaryService.Contracts.Positions;
using Shared.Core.Abstractions;

namespace DictionaryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;