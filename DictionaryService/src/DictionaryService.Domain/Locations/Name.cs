using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Locations;

public record Name
{
    private Name(string value) => Value = value;

    public string Value { get; }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 120 || value.Length < 3)
        {
            return GeneralErrors.ValueIsInvalid("Name", "Name must be between 3 and 120 characters");
        }

        return new Name(value);
    }
}