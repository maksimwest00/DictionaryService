using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Abstractions;

public interface ICommand;

public interface ICommandHandler<TResponse, in TCommand>
    where TCommand : ICommand
{
    Task<Result<TResponse, Error>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand>
{
    Task<UnitResult<Error>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken);
}