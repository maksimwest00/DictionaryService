using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Database;
using DictionaryService.Application.Departments;
using DictionaryService.Application.Locations.CreateLocation;
using DictionaryService.Application.Validation.ValidationExtensions;
using DictionaryService.Domain.Positions;
using DictionaryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace DictionaryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public CreatePositionHandler(
        IDepartmentRepository departmentRepository,
        IPositionRepository positionRepository,
        IValidator<CreatePositionCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _positionRepository = positionRepository;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> HandleAsync(
        CreatePositionCommand command,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var namePositionResult = Name.Create(command.Request.Name);
        var descriptionPositionResult = Description.Create(command.Request.Description);
        var departmentIds = command.Request.DepartmentIds;

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using var transactionScope = transactionScopeResult.Value;

        if (await _positionRepository.IsExistPositionNameAsync(namePositionResult.Value.Value, cancellationToken))
        {
            transactionScope.Rollback();
            return Result.Failure<Guid, Error>(Error.Failure(null, ["Position name is exist and active"]));
        }

        bool isDepartmentsExist = await _departmentRepository.ExistsAsync(
            departmentIds,
            cancellationToken);

        if (!isDepartmentsExist)
        {
            transactionScope.Rollback();
            return Result.Failure<Guid, Error>(Error.Failure(null, ["Departments not found"]));
        }

        var position = Position.Create(
            namePositionResult.Value,
            descriptionPositionResult.Value,
            departmentIds);

        var addPositionResult = await _positionRepository.AddAsync(position, cancellationToken);

        if (addPositionResult.IsFailure)
        {
            transactionScope.Rollback();
            return addPositionResult.Error;
        }

        var commitedResult = transactionScope.Commit();

        if (commitedResult.IsFailure)
        {
            return commitedResult.Error;
        }

        return addPositionResult;
    }
}