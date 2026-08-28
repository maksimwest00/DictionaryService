namespace DictionaryService.Contracts.Positions;

public record RenamePositionRequest(Guid PositionId, string Name);