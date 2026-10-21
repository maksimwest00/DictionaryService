namespace Shared.Core.Shared;

public record PagedResult<T>
{
    public List<T> Data { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }
}