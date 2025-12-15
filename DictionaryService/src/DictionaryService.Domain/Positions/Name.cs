using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Positions;

public record Name
{
    public const int MIN_NAME_LENGTH = 3;
    public const int MAX_NAME_LENGTH = 100;

    private Name(string value) => Value = value;

    public string Value { get; }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Position name");
        }

        if (value.Length is > MAX_NAME_LENGTH or < MIN_NAME_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid(
                "Position name",
                "Name must be between 3 and 100 characters");
        }

        return new Name(value);
    }
}