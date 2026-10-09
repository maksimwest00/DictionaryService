using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using DictionaryService.Domain.Shared;

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
                             Id,
                             Name,
                             Slug,
                             Path,
                             Depth,
                             HasChildren
                         FROM (
                             SELECT 
                                 D.id as Id,
                                 D.name as Name,
                                 D.identifier as Slug,
                                 D.path as Path,
                                 D.depth as Depth,
                                 (
                                     SELECT COUNT(*)
                                     FROM departments AS D1
                                     WHERE D1.parent_id = D.id
                                 ) > 0 as HasChildren
                             FROM departments AS D
                             WHERE parent_id IS NULL
                         ) AS DepartmentsData;
                 """);

        return dp.ToList();
    }
}