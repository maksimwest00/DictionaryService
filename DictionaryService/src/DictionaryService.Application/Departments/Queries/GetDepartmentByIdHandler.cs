using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Contracts.Departments.GetDepartmentById;
using DictionaryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.Application.Departments.Queries;

public class GetDepartmentByIdHandler : IQueryHandler<GetDepartmentByIdResponse, GetDepartmentByIdQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentByIdHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<GetDepartmentByIdResponse, Error>> HandleAsync(
        GetDepartmentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var location = await _readDbContext.DepartmentsRead
            .Where(x => x.Id == query.Id && x.IsActive)
            .Select(x => new GetDepartmentByIdResponse
            {
                Id = x.Id,
                Name = x.Name.Value,
                Identifier = x.Identifier.Value,
                ParentId = x.ParentId,
                Path = x.Path.Value,
                Depth = x.Depth,
                ChildrenCount = x.ChildrenCount,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
        {
            return Error.NotFound(null, ["Department not found"], query.Id);
        }

        return location;
    }
}