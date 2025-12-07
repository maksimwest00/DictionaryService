namespace DictionaryService.Domain.Departments;

public record Path
{
    private Path(string value) => Value = value;
    public string Value { get; }

    public static Path Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Invalid path");
        }

        return new Path(value);
    }
}