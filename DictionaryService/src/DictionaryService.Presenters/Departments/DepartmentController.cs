using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Departments.AddPosition;
using DictionaryService.Application.Departments.CreateDepartment;
using DictionaryService.Application.Departments.DeleteDepartment;
using DictionaryService.Application.Departments.DeletePosition;
using DictionaryService.Application.Departments.TransferDepartment;
using DictionaryService.Application.Departments.UpdateDepartmentLocations;
using DictionaryService.Contracts.Departments;
using DictionaryService.Domain.Shared;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DictionaryService.Presenters.Departments;

[ApiController]
[Route("/api/departments")]
public class DepartmentController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateDepartmentRequest request,
        [FromServices] ILogger<DepartmentController> logger,
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        CreateDepartmentCommand command = new(request);

        Result<Guid, Error> createResult = await handler.HandleAsync(command, cancellationToken);

        if (createResult.IsSuccess)
        {
            logger.LogInformation("Подразделение успешно создано с id: {CreateResultValue}", createResult.Value);
        }
        else
        {
            logger.LogInformation(
                "Ошибка создания подразделения: {ErrorMessage}",
                string.Join(',', createResult.Error.Messages));
        }

        return createResult.IsFailure ? createResult.Error.ToResponse() : Ok(Envelope.Ok(createResult.Value));
    }

    [HttpPut]
    [Route("/api/departments/{departmentId}/locations")]
    public async Task<IActionResult> UpdateLocationsAsync(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentLocationsRequest request,
        [FromServices] ILogger<DepartmentController> logger,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationsCommand> handler,
        CancellationToken cancellationToken)
    {
        UpdateDepartmentLocationsCommand command = new(departmentId, request);

        Result<Guid, Error> updateResult = await handler.HandleAsync(command, cancellationToken);

        if (updateResult.IsSuccess)
        {
            logger.LogInformation("Подразделение успешно обновлено {UpdateResultValue}", updateResult.Value);
        }
        else
        {
            logger.LogInformation(
                "Ошибка обновления подразделения: {ErrorMessage}",
                string.Join(',', updateResult.Error.Messages));
        }

        return updateResult.IsFailure ? updateResult.Error.ToResponse() : Ok(Envelope.Ok(updateResult.Value));
    }

    [HttpPut]
    [Route("/api/departments/{departmentId}/parent")]
    public async Task<IActionResult> TransferDepartmentAsync(
        [FromRoute] Guid departmentId,
        [FromBody] TransferDepartmentRequest request,
        [FromServices] ILogger<DepartmentController> logger,
        [FromServices] ICommandHandler<Guid, TransferDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        TransferDepartmentCommand command = new TransferDepartmentCommand(departmentId, request);

        var transferResult = await handler.HandleAsync(command, cancellationToken);

        if (transferResult.IsSuccess)
        {
            logger.LogInformation("Подразделение успешно перемещено {UpdateResultValue}", transferResult.Value);
        }
        else
        {
            logger.LogInformation(
                "Ошибка перемещения подразделения: {ErrorMessage}",
                string.Join(',', transferResult.Error.Messages));
        }

        return transferResult.IsFailure ? transferResult.Error.ToResponse() : Ok(Envelope.Ok(transferResult.Value));
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        [FromServices] ILogger<DepartmentController> logger,
        [FromServices] ICommandHandler<DeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        DeleteDepartmentCommand command = new(id);

        var deleteResult = await handler.HandleAsync(command, cancellationToken);

        if (deleteResult.IsSuccess)
        {
            logger.LogInformation("Подразделение успешно удалено id: {id}", command.Id);
        }
        else
        {
            logger.LogInformation(
                "Ошибка удаления подразделения: {ErrorMessage}",
                string.Join(',', deleteResult.Error.Messages));
        }

        return deleteResult.IsFailure ? deleteResult.Error.ToResponse() : Ok(Envelope.Ok());
    }

    [HttpPost("{deptId:Guid}/positions/{posId:Guid}")]
    public async Task<IActionResult> AddPositionAsync(
        [FromRoute] Guid deptId,
        [FromRoute] Guid posId,
        [FromServices] ILogger<DepartmentController> logger,
        [FromServices] ICommandHandler<Guid, AddPositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new AddPositionCommand(deptId, posId);

        var addPositionResult = await handler.HandleAsync(command, cancellationToken);

        if (addPositionResult.IsSuccess)
        {
            logger.LogInformation("Должность успешно привязана id: {id}", command.PosId);
        }
        else
        {
            logger.LogInformation(
                "Ошибка привязки должности: {ErrorMessage}",
                string.Join(',', addPositionResult.Error.Messages));
        }

        return addPositionResult.IsFailure ? addPositionResult.Error.ToResponse() : Ok(Envelope.Ok(addPositionResult.Value));
    }

    [HttpDelete("{deptId:Guid}/positions/{posId:Guid}")]
    public async Task<IActionResult> DeletePositionAsync(
        [FromRoute] Guid deptId,
        [FromRoute] Guid posId,
        [FromServices] ILogger<DepartmentController> logger,
        [FromServices] ICommandHandler<Guid, DeletePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeletePositionCommand(deptId, posId);

        var deletePositionResult = await handler.HandleAsync(command, cancellationToken);

        if (deletePositionResult.IsSuccess)
        {
            logger.LogInformation("Должность успешно отвязана id: {id}", command.PosId);
        }
        else
        {
            logger.LogInformation(
                "Ошибка отвязки должности: {ErrorMessage}",
                string.Join(',', deletePositionResult.Error.Messages));
        }

        return deletePositionResult.IsFailure ? deletePositionResult.Error.ToResponse() : Ok(Envelope.Ok(deletePositionResult.Value));
    }
}