using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Departments.GetDepartmentsTree;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentAncestors;

public class GetDepartmentAncestorsHandler : IQueryHandler<List<GetDepartmentsTreeResponse>, GetDepartmentAncestorsQuery>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;
    private readonly IDepartmentRepository _departmentRepository;

    public GetDepartmentAncestorsHandler(
        IReadDbConnectionFactory readDbConnectionFactory,
        IDepartmentRepository departmentRepository)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<List<GetDepartmentsTreeResponse>, Error>> HandleAsync(
        GetDepartmentAncestorsQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var departmentResult = await _departmentRepository.GetByIdAsync(query.Id, cancellationToken);

        if (departmentResult.IsFailure)
        {
            return departmentResult.Error;
        }

        string path = departmentResult.Value.Path.Value;

        var parameters = new DynamicParameters();
        parameters.Add("path", path);

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