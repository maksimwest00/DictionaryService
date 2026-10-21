using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Locations;

public record Name
{
    private const int MIN_NAME_LENGTH = 3;
    private const int MAX_NAME_LENGTH = 120;

    private Name(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsInvalid("location name", "Name must be not null");
        }

        if (value.Length is > MAX_NAME_LENGTH or < MIN_NAME_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("location name", "Name must be between 3 and 120 characters");
        }

        return new Name(value);
    }
}