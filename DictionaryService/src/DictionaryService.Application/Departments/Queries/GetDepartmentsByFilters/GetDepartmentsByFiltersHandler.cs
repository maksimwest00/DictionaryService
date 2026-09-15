using System.Data;
using System.Data.Common;
using CSharpFunctionalExtensions;
using Dapper;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Contracts.Departments.GetDepartmentsByFilters;
using DictionaryService.Contracts.Shared;
using DictionaryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace DictionaryService.Application.Departments.Queries.GetDepartmentsByFilters;

public class GetDepartmentsByFiltersHandler : IQueryHandler<PagedResult<DepartmentListItemResponse>, GetDepartmentsByFiltersQuery>
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

    private readonly IValidator<GetDepartmentsByFiltersQuery> _validator;
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetDepartmentsByFiltersHandler(
        IValidator<GetDepartmentsByFiltersQuery> validator,
        IReadDbConnectionFactory readDbConnectionFactory)
    {
        _validator = validator;
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    public async Task<Result<PagedResult<DepartmentListItemResponse>, Error>> HandleAsync(
        GetDepartmentsByFiltersQuery query,
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
            conditions.Add("name ILIKE @search");
            parameters.Add("@search", $"%{query.Search}%");
        }

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            parameters.Add("@sortBy", query.SortBy);
        }

        if (!string.IsNullOrWhiteSpace(query.SortDir))
        {
            parameters.Add("@sortDir", query.SortDir);
        }

        parameters.Add("@pageSize", query.PageSize);
        parameters.Add("@offset", (query.Page - 1) * query.PageSize);

        string whereClause = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;

        string sortColumn = SortColumns.TryGetValue(query.SortBy ?? string.Empty, out string? col)
            ? col
            : "created_at"; // дефолт

        string sortDir = SortDirections.TryGetValue(query.SortDir ?? string.Empty, out string? dir)
            ? dir
            : "DESC";

        string orderByClause = $"ORDER BY {sortColumn} {sortDir}";

        string countSql = $"""
                           SELECT COUNT(*)
                           FROM departments
                           {whereClause}
                           """;

        long totalCount = await connection.ExecuteScalarAsync<long>(
            countSql, parameters);

        var departments =
            await connection.QueryAsync<DepartmentListItemResponse>(
                $"""
                     SELECT id
                            ,name
                            ,path
                            ,created_at
                     FROM departments
                     {whereClause}
                     {orderByClause}
                     LIMIT @pageSize OFFSET @offset
                 """,
                param: parameters);


        var pagedResult = new PagedResult<DepartmentListItemResponse>()
        {
            Data = departments.ToList(),
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        };

        return pagedResult;
    }
}