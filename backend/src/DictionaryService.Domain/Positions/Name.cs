using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Positions;

public record Name
{
    private const int NAME_MIN_LENGTH = 3;
    public const int NAME_MAX_LENGTH = 100;

    private Name(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Position name");
        }

        if (value.Length is > NAME_MAX_LENGTH or < NAME_MIN_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid(
                "Position name",
                "Name must be between 3 and 100 characters");
        }

        return new Name(value);
    }
}