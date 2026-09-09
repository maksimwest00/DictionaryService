using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Locations.Commands.CreateLocation;
using DictionaryService.Application.Locations.Commands.DeleteLocation;
using DictionaryService.Application.Locations.Queries.GetLocationById;
using DictionaryService.Application.Locations.Queries.GetTop;
using DictionaryService.Contracts.Locations.CreateLocation;
using DictionaryService.Contracts.Locations.GetLocationById;
using DictionaryService.Contracts.Locations.GetTop;
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

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid id,
        [FromServices] ILogger<LocationController> logger,
        [FromServices] IQueryHandler<GetLocationByIdResponse, GetLocationByIdQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationByIdQuery(id);

        var result = await handler.HandleAsync(query, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Локация успешно получена id: {id}", query.Id);
        }
        else
        {
            logger.LogInformation(
                "Ошибка получения локации: {ErrorMessage}",
                string.Join(',', result.Error.Messages));
        }

        return result.IsFailure ? result.Error.ToResponse() : Ok(Envelope.Ok(result.Value));
    }

    [HttpGet("/top")]
    public async Task<IActionResult> GetTop(
        [FromServices] ILogger<LocationController> logger,
        [FromServices] IQueryHandler<GetTopResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Топ локаций успешно получен");
        }
        else
        {
            logger.LogInformation(
                "Ошибка получения топа локаций: {ErrorMessage}",
                string.Join(',', result.Error.Messages));
        }

        return result.IsFailure ? result.Error.ToResponse() : Ok(Envelope.Ok(result.Value));
    }
}