using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using Shared.Core.Abstractions;
using Shared.Kernel;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentAncestors;

public class GetDepartmentAncestorsHandler :
    IQueryHandler<List<GetDepartmentsTreeResponse>, GetDepartmentAncestorsQuery>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetDepartmentAncestorsHandler(IReadDbConnectionFactory readDbConnectionFactory)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<List<GetDepartmentsTreeResponse>, Error>> HandleAsync(
        GetDepartmentAncestorsQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        string? pathResult = await connection.QuerySingleOrDefaultAsync<string>(
            """
                    SELECT path
                    FROM departments
                    WHERE id = @Id
                      AND is_active = true;
                """,
            param: new { Id = query.Id });

        if (pathResult == null)
        {
            return Error.NotFound(
                "record.not.found",
                ["Department not found"],
                query.Id);
        }

        var parameters = new DynamicParameters();
        parameters.Add("path", pathResult);

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
                        WHERE D.path @> @path::ltree  
                          AND D.path != @path::ltree
                          AND D.is_active = true
                        GROUP BY D.id, D.name, D.identifier, D.path, D.depth, D.parent_id
                        ORDER BY D.depth;
                    """,
                param: parameters);

        return dp.ToList();
    }
}