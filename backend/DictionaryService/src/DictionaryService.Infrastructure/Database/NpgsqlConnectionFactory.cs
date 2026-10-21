using System.Data.Common;
using DictionaryService.Application.Database;
using Npgsql;

namespace DictionaryService.Infrastructure.Database;

public class NpgsqlReadDbConnectionFactory : IReadDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlReadDbConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }
}