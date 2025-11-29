namespace DictionaryService.Domain.Departments;

public record Path
{
    public string Value { get; private set; }

    private Path()
    {
    }

    public static Path Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Invalid path");
        return new Path { Value = value };
    }
}