using System.Text.Json.Serialization;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Presenters.ResponseExtensions;

public record Envelope
{
    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope()
    {

    }

    private Envelope(
        object? result,
        Errors errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.UtcNow;
    }

    public object? Result { get; }

    public Errors Errors { get; }

    public static Envelope Ok(object? result = null) =>
        new(result, new Errors([]));

    public static Envelope Error(Error error) => new(null, error.ToErrors());
}