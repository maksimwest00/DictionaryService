using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using DictionaryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.Application.Departments.Queries.GetChildrenByRootId;

public class GetChildrenByRootIdHandler : IQueryHandler<List<GetDepartmentsTreeResponse>, GetChildrenByRootIdQuery>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;
    private readonly IReadDbContext _readDbContext;

    public GetChildrenByRootIdHandler(
        IReadDbConnectionFactory readDbConnectionFactory,
        IReadDbContext readDbContext)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
        _readDbContext = readDbContext;
    }

    public async Task<Result<List<GetDepartmentsTreeResponse>, Error>> HandleAsync(
        GetChildrenByRootIdQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("RootId", query.Id);

        bool departmentExist = await _readDbContext.DepartmentsRead.AnyAsync(
            d => d.Id == query.Id, cancellationToken);

        if (!departmentExist)
        {
            return Error.NotFound("record.not.found", ["Department not found"], query.Id);
        }

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
                        WHERE D.parent_id = @RootId AND D.is_active = true
                        GROUP BY D.id, D.name, D.identifier, D.path, D.depth, D.parent_id
                    """,
                param: parameters);

        return dp.ToList();
    }
}