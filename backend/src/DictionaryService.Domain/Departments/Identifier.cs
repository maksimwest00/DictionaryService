using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Departments;

public record Identifier
{
    public const int IDENTIFIER_MIN_LENGTH = 3;
    public const int IDENTIFIER_MAX_LENGTH = 150;

    private Identifier(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("department identifier");
        }

        if (value.Length is > IDENTIFIER_MAX_LENGTH or < IDENTIFIER_MIN_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("department identifier", "Identifier must be between 3 and 150 characters");
        }

        if (!Regex.IsMatch(value, @"^[a-zA-Z]*$"))
        {
            return GeneralErrors.ValueIsInvalid("department identifier", "Identifier must be in Latin characters");
        }

        return new Identifier(value);
    }
}