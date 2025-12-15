using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Departments;

public record Name
{
    public const int NAME_MIN_LENGTH = 3;
    public const int NAME_MAX_LENGTH = 150;

    private Name(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("department name");
        }

        if (value.Length is > NAME_MAX_LENGTH or < NAME_MIN_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("department name", "Name must be between 3 and 150 characters");
        }

        return new Name(value);
    }
}