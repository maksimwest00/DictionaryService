namespace DictionaryService.Contracts.Shared;

public record CursorResult<T>
{
    public List<T> Data { get; set; }

    public string? NextCursor { get; set; }
}
