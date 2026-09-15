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

        long? totalCount = null;

        string whereClause = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;

        string orderByClause = !string.IsNullOrWhiteSpace(query.SortBy)
            ? $"ORDER BY {query.SortBy} {(!string.IsNullOrWhiteSpace(query.SortDir)
                                            ? query.SortDir : string.Empty)}"
            : string.Empty;

        var departments =
            await connection.QueryAsync<DepartmentListItemResponse, long, DepartmentListItemResponse>(
                $"""
                        SELECT id
                               ,name
                               ,path
                               ,created_at
                               ,COUNT(*) OVER() AS total_count
                        FROM departments
                        {whereClause}
                        {orderByClause}
                        LIMIT @pageSize OFFSET @offset
                    """,
                splitOn: "total_count",
                map: (department, count) =>
                {
                    if (totalCount is null)
                    {
                        totalCount = count;
                    }
                    return department;
                },
                param: parameters);


        var pagedResult = new PagedResult<DepartmentListItemResponse>()
        {
            Data = departments.ToList(),
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount ?? 0,
        };

        return pagedResult;
    }
}