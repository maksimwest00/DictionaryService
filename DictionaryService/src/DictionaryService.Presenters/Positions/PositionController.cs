using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Positions.CreatePosition;
using DictionaryService.Contracts;
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
}