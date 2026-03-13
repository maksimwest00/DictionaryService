using System.Text.Json.Serialization;

namespace DictionaryService.Domain.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    VALIDATION,
    NOT_FOUND,
    FAILURE,
    CONFLICT,
}