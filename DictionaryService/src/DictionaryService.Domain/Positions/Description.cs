namespace DictionaryService.Domain.Positions;

public record Description
{
    private Description(string? value) => Value = value;
    public static Description EmptyDescription => new((string?)null);

    public string? Value { get; }

    public static Description Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 1000)
        {
            throw new ArgumentException("Description must be between 3 and 1000 characters");
        }

        return new Description(value);
    }
}