using System.Text.RegularExpressions;

namespace DictionaryService.Domain.Departments;

public record Identifier
{
    private Identifier(string value) => Value = value;

    public string Value { get; }

    public static Identifier Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 150 || value.Length < 3)
        {
            throw new ArgumentException("Identifier must be between 3 and 150 characters");
        }

        if (!Regex.IsMatch(value, @"^[a-zA-Z]*$"))
        {
            throw new ArgumentException("Identifier must be in Latin characters");
        }

        return new Identifier(value);
    }
}