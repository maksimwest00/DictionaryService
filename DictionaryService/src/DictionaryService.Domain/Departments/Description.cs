namespace DictionaryService.Domain.Departments;

public record Description
{
    public static Description EmptyDescription => new Description((string?)null);

    private Description(string? value)
    {
        Value = value;
    }

    public string? Value { get; private set; }

    public static Description Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 1000 || value.Length < 3)
        {
            throw new ArgumentException("Description must be between 3 and 1000 characters");
        }

        return new Description(value);
    }
}