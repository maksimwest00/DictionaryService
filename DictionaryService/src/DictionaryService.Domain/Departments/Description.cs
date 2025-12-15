using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Departments;

public record Description
{
    public const int MAX_DESCRIPTION_LENGTH = 1000;
    public const int MIN_DESCRIPTION_LENGTH = 3;

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
            return GeneralErrors.ValueIsRequired("Description");
        }

        if (value.Length > MAX_DESCRIPTION_LENGTH || value.Length < MIN_DESCRIPTION_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid(
                "Description",
                "Description must be between 3 and 1000 characters");
        }

        return new Description(value);
    }
}