using System.Text.Json.Serialization;

namespace DictionaryService.Domain.Shared;

public record Error
{
    [JsonConstructor]
    private Error(
        string code,
        string[] messages,
        ErrorType type,
        string? invalidField = null)
    {
        Code = code;
        Messages = messages;
        Type = type;
        InvalidField = invalidField;
    }

    public string Code { get; }

    public string[] Messages { get; }

    public ErrorType Type { get; }

    public string? InvalidField { get; }

    public static Error NotFound(
        string? code,
        string[] messages,
        Guid? id)
        => new(code ?? "record.not.found", messages, ErrorType.NOT_FOUND);

    public static Error Validation(
        string? code,
        string[] messages,
        string? invalidField = null)
        => new(code ?? "value.is.invalid", messages, ErrorType.VALIDATION, invalidField);

    public static Error Conflict(
        string? code,
        string[] messages)
        => new(code ?? "value.is.conflict", messages, ErrorType.CONFLICT);

    public static Error Failure(string? code, string[] messages)
        => new(code ?? "failure", messages, ErrorType.FAILURE);

    public Errors ToErrors()
    {
        return new Errors([this]);
    }
}