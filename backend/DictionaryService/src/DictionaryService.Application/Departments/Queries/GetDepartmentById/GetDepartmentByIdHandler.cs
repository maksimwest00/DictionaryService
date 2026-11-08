using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Departments.GetDepartmentById;
using Shared.Core.Abstractions;
using Shared.Kernel;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentById;

public class GetDepartmentByIdHandler : IQueryHandler<GetDepartmentByIdResponse, GetDepartmentByIdQuery>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetDepartmentByIdHandler(IReadDbConnectionFactory readDbConnectionFactory)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<GetDepartmentByIdResponse, Error>> HandleAsync(
        GetDepartmentByIdQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var department = await connection.QuerySingleOrDefaultAsync<GetDepartmentByIdResponse>(
            """
                    SELECT
                        id as Id,
                        name as Name,
                        identifier as Identifier,
                        parent_id as ParentId,
                        path as Path,
                        depth as Depth,
                        children_count as ChildrenCount,
                        is_active as IsActive,
                        created_at as CreatedAt,
                        updated_at as UpdatedAt
                    FROM departments
                    WHERE id = @Id
                      AND is_active = true
                """,
            param: new { Id = query.Id });

        if (department is null)
        {
            return Error.NotFound(
                "department.not.found",
                ["Department not found"],
                query.Id);
        }

        return department;
    }
}