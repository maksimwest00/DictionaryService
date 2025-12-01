namespace DictionaryService.Domain.Positions;

public record Name
{
    private Name(string value) => Value = value;

    public string Value { get; }

    public static Name Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 100 || value.Length < 3)
        {
            throw new ArgumentException("Name must be between 3 and 100 characters");
        }

        return new Name(value);
    }
}