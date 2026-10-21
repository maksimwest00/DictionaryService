using System.Text.Json.Serialization;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Presenters.ResponseExtensions;

public record Envelope
{
    public DateTime TimeGenerated { get; }

    private Envelope(object? result)
    {
        Result = result;
        TimeGenerated = DateTime.UtcNow;
    }

    [JsonConstructor]
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

public record Envelope<T>
{
    public DateTime TimeGenerated { get; }

    private Envelope(T? result)
    {
        Result = result;
        TimeGenerated = DateTime.UtcNow;
    }

    [JsonConstructor]
    private Envelope(
        T? result,
        Errors errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.UtcNow;
    }

    public T? Result { get; }

    public Errors Errors { get; }

    public static Envelope<T> Ok(T? result = default) =>
        new(result, new Errors([]));

    public static Envelope<T> Error(Error error) => new(default, error.ToErrors());
}