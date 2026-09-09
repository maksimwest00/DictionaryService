using System.Data.Common;

namespace DictionaryService.Application.Database;

public interface IReadDbConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}