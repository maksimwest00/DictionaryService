namespace DictionaryService.Domain.Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? field, string message)
    {
        var label = field ?? "value";
        return Error.Validation($"{label} is invalid", [$"{message}"], $"{field}");
    }

    public static Error ValueIsRequired(string? field)
    {
        var label = field ?? "value";
        return Error.Validation($"{label} is required", [$"{field} required"], $"{field}");
    }
}