using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentsTreeBySearch;

public class GetDepartmentsTreeBySearchHandler : IQueryHandler<List<GetDepartmentsTreeResponse>,GetDepartmentsTreeBySearchQuery>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetDepartmentsTreeBySearchHandler(IReadDbConnectionFactory readDbConnectionFactory)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<List<GetDepartmentsTreeResponse>, Error>> HandleAsync(
        GetDepartmentsTreeBySearchQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("search", query.search);

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
                            COALESCE(COUNT(DC.id), 0) > 0 as HasChildren
                        FROM departments AS D
                        LEFT JOIN departments AS DC ON DC.parent_id = D.id AND DC.is_active = true
                        WHERE D.parent_id IS NULL
                          AND D.name ILIKE '%' || @search || '%'
                          AND D.is_active = true
                        GROUP BY D.id, D.name, D.identifier, D.path, D.depth, D.parent_id
                """,
                param: parameters);

        return dp.ToList();
    }
}