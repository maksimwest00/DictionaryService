using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Domain.Locations;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Application.Locations.DeleteLocation;

public class DeleteLocationHandler : ICommandHandler<DeleteLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;

    public DeleteLocationHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Error>> HandleAsync(
        DeleteLocationCommand command,
        CancellationToken cancellationToken)
    {
        var locationId = command.Id;

        Result<Location, Error> locationResult =
            await _locationRepository.GetByIdAsync(
                locationId,
                cancellationToken);

        if (locationResult.IsFailure)
        {
            return locationResult.Error;
        }

        Location location = locationResult.Value;

        Result<ITransactionScope, Error> transactionScopeResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using ITransactionScope transactionScope = transactionScopeResult.Value;

        location.Deactivate();

        UnitResult<Error> saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }

        UnitResult<Error> commitedResult = transactionScope.Commit();

        if (commitedResult.IsFailure)
        {
            return commitedResult.Error;
        }

        return UnitResult.Success<Error>();
    }
}