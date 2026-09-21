using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Contracts.Locations.GetLocationsByFilters;
using DictionaryService.Contracts.Shared;
using DictionaryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace DictionaryService.Application.Locations.Queries.GetLocationsByFilters;

public class GetLocationsByFiltersHandler : IQueryHandler<PagedResult<LocationListItemResponse>, GetLocationsByFiltersQuery>
{
    private static readonly Dictionary<string, string> _sortColumns =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "name",
            ["created_at"] = "created_at",
            ["departmentCount"] = "department_count",
        };

    private static readonly Dictionary<string, string> _sortDirections =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["asc"] = "ASC",
            ["desc"] = "DESC",
        };

    private readonly IValidator<GetLocationsByFiltersQuery> _validator;
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetLocationsByFiltersHandler(
        IValidator<GetLocationsByFiltersQuery> validator,
        IReadDbConnectionFactory readDbConnectionFactory)
    {
        _validator = validator;
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<PagedResult<LocationListItemResponse>, Error>> HandleAsync(
        GetLocationsByFiltersQuery query,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        using IDbConnection connection =
            await _readDbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            conditions.Add("L.name ILIKE @search");
            parameters.Add("@search", $"%{query.Search}%");
        }

        if (query.MinDepartmentCount.HasValue
            && query.MinDepartmentCount > 0)
        {
            parameters.Add("@minDepartmentCount", query.MinDepartmentCount.Value);
        }

        string whereClause2 = query.MinDepartmentCount.HasValue
                              && query.MinDepartmentCount > 0
            ? "HAVING COUNT(D.id) >= @minDepartmentCount" : string.Empty;

        parameters.Add("@pageSize", query.PageSize);
        parameters.Add("@offset", (query.Page - 1) * query.PageSize);

        string whereClause = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;

        string sortColumn = _sortColumns.TryGetValue(query.SortBy ?? string.Empty, out string? col)
            ? col
            : "created_at";

        string sortDir = _sortDirections.TryGetValue(query.SortDir ?? string.Empty, out string? dir)
            ? dir
            : "DESC";

        string orderByClause = $"ORDER BY {sortColumn} {sortDir}";

        long totalCount = await connection.ExecuteScalarAsync<long>(
            $"""
                WITH filtered AS (
                    SELECT L.id,
                           L.name,
                           L.created_at,
                           COUNT(D.id) AS department_count,
                           L.city,
                           L.street,
                           L.building,
                           L.room_number
                    FROM locations AS L
                             LEFT JOIN department_locations AS DL ON L.id = DL.location_id
                             INNER JOIN departments AS D ON DL.department_id = D.id
                    {whereClause}
                    GROUP BY L.id
                    {whereClause2}
                )
                
                SELECT COUNT(*) AS total_count
                FROM filtered;
             """, parameters);

        var locations =
            await connection.QueryAsync<LocationListItemResponse, AddressDto, LocationListItemResponse>(
                $"""
                        WITH filtered AS (
                            SELECT L.id,
                                   L.name,
                                   L.created_at,
                                   COUNT(D.id) AS department_count,
                                   L.city,
                                   L.street,
                                   L.building,
                                   L.room_number
                            FROM locations AS L
                                     LEFT JOIN department_locations AS DL ON L.id = DL.location_id
                                     INNER JOIN departments AS D ON DL.department_id = D.id
                            {whereClause}
                            GROUP BY L.id
                            {whereClause2}
                        )
                        
                        SELECT  id,
                                name,
                                created_at,
                                department_count,
                                city,
                                street,
                                building,
                                room_number
                        FROM filtered
                        {orderByClause}
                        LIMIT @pageSize
                        OFFSET @offset;
                    """,
                splitOn: "city",
                map: (locationDto, addressDto) =>
                {
                    return locationDto with { Address = addressDto };
                },
                param: parameters);

        var response = new PagedResult<LocationListItemResponse>
        {
            Data = locations.ToList(),
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        };

        return response;
    }
}