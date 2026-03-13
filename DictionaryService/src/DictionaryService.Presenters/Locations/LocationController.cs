using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Locations;
using DictionaryService.Application.Locations.CreateLocation;
using DictionaryService.Contracts.Locations;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DictionaryService.Presenters.Locations;

[ApiController]
[Route("/api/[controller]")]
public class LocationController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateLocationRequest request,
        [FromServices] ILogger<LocationController> logger,
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);

        var createResult = await handler.HandleAsync(command, cancellationToken);

        if (createResult.IsSuccess)
        {
            logger.LogInformation("Локация успешно создана с id: {CreateResultValue}", createResult.Value);
        }
        else
        {
            logger.LogInformation("Ошибка создания локации: {ErrorMessage}", string.Join(',', createResult.Error.Messages));
        }

        return createResult.IsFailure ? createResult.Error.ToResponse() : Ok(Envelope.Ok(createResult.Value));
    }
}