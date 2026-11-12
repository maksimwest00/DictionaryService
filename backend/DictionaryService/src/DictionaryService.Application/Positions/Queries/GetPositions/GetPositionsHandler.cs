using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Positions;
using DictionaryService.Contracts.Shared;
using FluentValidation;
using Shared.Core.Abstractions;
using Shared.Core.Validation.ValidationExtensions;
using Shared.Kernel;

namespace DictionaryService.Application.Positions.Queries.GetPositions;

public class GetPositionsHandler : IQueryHandler<CursorResult<PositionListItemResponse>, GetPositionsQuery>
{
    private static readonly IReadOnlyDictionary<string, string> SortColumns =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "name",
            ["created_at"] = "created_at",
        };

    private static readonly IReadOnlyDictionary<string, string> SortDirections =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["asc"] = "ASC",
            ["desc"] = "DESC",
        };

    private readonly IValidator<GetPositionsQuery> _validator;
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetPositionsHandler(
        IValidator<GetPositionsQuery> validator,
        IReadDbConnectionFactory readDbConnectionFactory)
    {
        _validator = validator;
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<CursorResult<PositionListItemResponse>, Error>> HandleAsync(
        GetPositionsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("is_active = true");

        if (query.DepartmentId.HasValue)
        {
            conditions.Add("EXISTS (SELECT 1 FROM department_positions dp WHERE dp.position_id = p.id AND dp.department_id = @departmentId)");
            parameters.Add("@departmentId", query.DepartmentId.Value);
        }

        string whereClause = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;

        string orderByClause = "ORDER BY p.created_at ASC, p.id";

        int pageSize = query.PageSize;
        int fetchSize = pageSize + 1;
        string cursorSql = string.Empty;

        if (!string.IsNullOrWhiteSpace(query.Cursor))
        {
            var cursorParts = query.Cursor.Split('|');
            if (cursorParts.Length == 2)
            {
                var cursorCreatedAt = DateTime.Parse(cursorParts[0]);
                var cursorId = Guid.Parse(cursorParts[1]);
                cursorSql = "AND (p.created_at > @cursorCreatedAt OR (p.created_at = @cursorCreatedAt AND p.id > @cursorId))";
                parameters.Add("@cursorCreatedAt", cursorCreatedAt);
                parameters.Add("@cursorId", cursorId);
            }
        }

        string sql = $"""
                     SELECT p.id
                            ,p.name
                            ,p.description
                            ,p.created_at as CreatedAt
                            ,p.updated_at as UpdatedAt
                            ,p.is_active as IsActive
                            ,(SELECT COUNT(*) FROM department_positions dp WHERE dp.position_id = p.id) as DepartmentCount
                     FROM positions p
                     {whereClause}
                     {cursorSql}
                     {orderByClause}
                     LIMIT @fetchSize
                     """;

        parameters.Add("@fetchSize", fetchSize);

        var positions =
            await connection.QueryAsync<PositionListItemResponse>(
                sql,
                param: parameters);

        var positionList = positions.ToList();

        string? nextCursor = null;

        if (positionList.Count > pageSize)
        {
            positionList = positionList.Take(pageSize).ToList();
            var lastPosition = positionList.Last();
            nextCursor = $"{lastPosition.CreatedAt:o}|{lastPosition.Id}";
        }

        var cursorResult = new CursorResult<PositionListItemResponse>()
        {
            Data = positionList,
            NextCursor = nextCursor
        };

        return cursorResult;
    }
}
