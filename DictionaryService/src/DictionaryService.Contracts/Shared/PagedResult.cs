namespace DictionaryService.Contracts.Shared;

public class PagedResult<T>
{
    public List<T> Data { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }
}