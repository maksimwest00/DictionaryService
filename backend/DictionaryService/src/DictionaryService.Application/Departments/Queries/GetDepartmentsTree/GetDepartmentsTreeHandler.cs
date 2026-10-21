using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using Shared.Core.Abstractions;
using DictionaryService.Application.Database;
using Shared.Core.Database;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using Shared.Kernel;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentsTree;

public class GetDepartmentsTreeHandler : IQueryHandler<List<GetDepartmentsTreeResponse>>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetDepartmentsTreeHandler(IReadDbConnectionFactory readDbConnectionFactory)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<List<GetDepartmentsTreeResponse>, Error>> HandleAsync(
        CancellationToken cancellationToken)
    {
        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var dp =
            await connection.QueryAsync<GetDepartmentsTreeResponse>(
                """
                    SELECT
                        D.id as Id,
                        D.name as Name,
                        D.identifier as Slug,
                        D.path as Path,
                        D.depth as Depth,
                        D.parent_id as ParentId,
                        D.children_count > 0 as HasChildren
                    FROM departments AS D
                    WHERE D.parent_id IS NULL AND D.is_active = true
                 """);

        return dp.ToList();
    }
}