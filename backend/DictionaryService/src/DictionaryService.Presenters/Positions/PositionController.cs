using DictionaryService.Application.Positions.Commands.CreatePosition;
using DictionaryService.Application.Positions.Commands.DeletePosition;
using DictionaryService.Application.Positions.Commands.RenamePosition;
using DictionaryService.Application.Positions.Queries.GetPositions;
using DictionaryService.Contracts.Positions;
using DictionaryService.Contracts.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Core.Abstractions;
using Shared.Framework.ResponseExtensions;

namespace DictionaryService.Presenters.Positions;

[ApiController]
[Route("/api/positions")]
public class PositionController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePositionRequest request,
        [FromServices] ILogger<PositionController> logger,
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);

        var createResult = await handler.HandleAsync(command, cancellationToken);

        if (createResult.IsSuccess)
        {
            logger.LogInformation("Должность успешно создана с id: {CreateResultValue}", createResult.Value);
        }
        else
        {
            logger.LogInformation("Ошибка создания должности: {ErrorMessage}", string.Join(',', createResult.Error.Messages));
        }

        return createResult.IsFailure ? createResult.Error.ToResponse() : Ok(Envelope.Ok(createResult.Value));
    }

    [HttpPatch("{id:Guid}")]
    public async Task<IActionResult> RenameAsync(
        [FromRoute] Guid id,
        [FromBody] RenamePositionRequest request,
        [FromServices] ILogger<PositionController> logger,
        [FromServices] ICommandHandler<Guid, RenamePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new RenamePositionCommand(id, request);

        var renameResult = await handler.HandleAsync(command, cancellationToken);

        if (renameResult.IsSuccess)
        {
            logger.LogInformation("Должность успешно переименована id: {PositionId}", renameResult.Value);
        }
        else
        {
            logger.LogInformation(
                "Ошибка переименования должности: {ErrorMessage}",
                string.Join(',', renameResult.Error.Messages));
        }

        return renameResult.IsFailure ? renameResult.Error.ToResponse() : Ok(Envelope.Ok(renameResult.Value));
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        [FromServices] ILogger<PositionController> logger,
        [FromServices] ICommandHandler<DeletePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        DeletePositionCommand command = new(id);

        var deleteResult = await handler.HandleAsync(command, cancellationToken);

        if (deleteResult.IsSuccess)
        {
            logger.LogInformation("Должность успешно удалена id: {PositionId}", command.Id);
        }
        else
        {
            logger.LogInformation(
                "Ошибка удаления должности: {ErrorMessage}",
                string.Join(',', deleteResult.Error.Messages));
        }

        return deleteResult.IsFailure ? deleteResult.Error.ToResponse() : Ok(Envelope.Ok());
    }

    [HttpGet]
    public async Task<IActionResult> GetPositionsAsync(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize,
        [FromQuery] Guid? departmentId,
        [FromServices] ILogger<PositionController> logger,
        [FromServices] IQueryHandler<CursorResult<PositionListItemResponse>, GetPositionsQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetPositionsQuery(cursor, pageSize, departmentId);

        var result = await handler.HandleAsync(query, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Должности успешно получены. Cursor: {Cursor}, PageSize: {PageSize}, DepartmentId: {DepartmentId}", cursor, pageSize, departmentId);
        }
        else
        {
            logger.LogInformation(
                "Ошибка получения должностей: {ErrorMessage}",
                string.Join(',', result.Error.Messages));
        }

        return result.IsFailure ? result.Error.ToResponse() : Ok(Envelope.Ok(result.Value));
    }
}