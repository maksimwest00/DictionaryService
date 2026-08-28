using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Positions.CreatePosition;
using DictionaryService.Application.Positions.RenamePosition;
using DictionaryService.Contracts.Positions;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

    [HttpPatch]
    public async Task<IActionResult> RenameAsync(
        [FromBody] RenamePositionRequest request,
        [FromServices] ILogger<PositionController> logger,
        [FromServices] ICommandHandler<Guid, RenamePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new RenamePositionCommand(request);

        var renameResult = await handler.HandleAsync(command, cancellationToken);

        if (renameResult.IsSuccess)
        {
            logger.LogInformation("Должность успешно переименована id: {CreateResultValue}", renameResult.Value);
        }
        else
        {
            logger.LogInformation(
                "Ошибка переименования должности: {ErrorMessage}",
                string.Join(',', renameResult.Error.Messages));
        }

        return renameResult.IsFailure ? renameResult.Error.ToResponse() : Ok(Envelope.Ok(renameResult.Value));
    }
}