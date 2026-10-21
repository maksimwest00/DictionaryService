using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace Shared.Core.Abstractions;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse, Error>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken);
}

public interface IQueryHandler<TResponse>
{
    Task<Result<TResponse, Error>> HandleAsync(CancellationToken cancellationToken);
}