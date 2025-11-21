namespace DictionaryService.Domain.Departments;

public record Name
{
    private Name(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static Name Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            (value.Length > 150 || value.Length < 3))
        {
            throw new ArgumentException("Name must be between 3 and 150 characters");
        }

        return new Name(value);
    }
}