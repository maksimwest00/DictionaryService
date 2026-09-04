using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Locations;
using DictionaryService.Application.Locations.CreateLocation;
using DictionaryService.Application.Locations.DeleteLocation;
using DictionaryService.Contracts.Locations;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DictionaryService.Presenters.Locations;

[ApiController]
[Route("/api/locations")]
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

    // DELETE /locations/{id} - удалить локацию
    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        [FromServices] ILogger<LocationController> logger,
        [FromServices] ICommandHandler<DeleteLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteLocationCommand(id);

        var deleteResult = await handler.HandleAsync(command, cancellationToken);

        if (deleteResult.IsSuccess)
        {
            logger.LogInformation("Локация успешно удалена id: {id}", command.Id);
        }
        else
        {
            logger.LogInformation(
                "Ошибка удаления локации: {ErrorMessage}",
                string.Join(',', deleteResult.Error.Messages));
        }

        return deleteResult.IsFailure ? deleteResult.Error.ToResponse() : Ok(Envelope.Ok());
    }
}