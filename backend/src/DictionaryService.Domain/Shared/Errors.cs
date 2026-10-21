using System.Collections;
using System.Text.Json.Serialization;

namespace DictionaryService.Domain.Shared;

[JsonConverter(typeof(ErrorsJsonConverter))]
public class Errors : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    [JsonConstructor]
    public Errors(List<Error> errors)
    {
        _errors = [..errors];
    }

    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}