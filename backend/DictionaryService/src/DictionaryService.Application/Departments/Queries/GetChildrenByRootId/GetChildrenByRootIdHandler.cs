using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.Kernel;

namespace DictionaryService.Application.Departments.Queries.GetChildrenByRootId;

public class GetChildrenByRootIdHandler : IQueryHandler<List<GetDepartmentsTreeResponse>, GetChildrenByRootIdQuery>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetChildrenByRootIdHandler(IReadDbConnectionFactory readDbConnectionFactory)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<List<GetDepartmentsTreeResponse>, Error>> HandleAsync(
        GetChildrenByRootIdQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("RootId", query.Id);

        var parentExists = await connection.QueryFirstOrDefaultAsync<Guid?>(
            "SELECT id FROM departments WHERE id = @RootId AND is_active = true",
            param: parameters);

        if (parentExists == null)
        {
            return Error.NotFound(
                "record.not.found",
                ["Department not found"],
                query.Id);
        }

        var departments =
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
                        WHERE D.parent_id = @RootId AND D.is_active = true
                        GROUP BY D.id, D.name, D.identifier, D.path, D.depth, D.parent_id
                    """,
                param: parameters);

        return departments.ToList();
    }
}