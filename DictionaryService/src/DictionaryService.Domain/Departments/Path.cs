namespace DictionaryService.Domain.Departments;

public record Path
{
    private const char SEPARATOR = '/';

    private Path(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Path CreateParent(Identifier identifier)
    {
        return new Path(identifier.Value);
    }

    public Path CreateChild(Identifier childIdentifier)
    {
        return new Path(Value + SEPARATOR + childIdentifier.Value);
    }
}