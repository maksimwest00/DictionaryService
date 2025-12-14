using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Positions;

public record Description
{
    public const int MAX_DESCRIPTION_LENGTH = 1000;

    private Description(string? value)
    {
        Value = value;
    }

    public static Description EmptyDescription => new((string?)null);

    public string? Value { get; }

    public static Result<Description, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Position description");
        }

        if (value.Length > MAX_DESCRIPTION_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid(
                "Position description",
                "Description must be between 3 and 1000 characters");
        }

        return new Description(value);
    }
}