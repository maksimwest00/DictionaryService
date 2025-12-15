using DictionaryService.Application.Locations;
using DictionaryService.Contracts.Locations;
using DictionaryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DictionaryService.Presenters.Locations;

[ApiController]
[Route("/api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationSerivce _locationService;

    public LocationController(ILocationSerivce locationSerivce)
    {
        _locationService = locationSerivce;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateLocationDto request,
        [FromServices] ILogger<LocationController> logger,
        CancellationToken cancellationToken)
    {
        var createResult = await _locationService.CreateAsync(request, cancellationToken);

        if (createResult.IsSuccess)
        {
            logger.LogInformation("Локация успешно создана с id: {CreateResultValue}", createResult.Value);
        }
        else
        {
            logger.LogInformation("Ошибка создания локации: {error}", createResult.Error.Message);
        }

        return createResult.IsFailure ? createResult.Error.ToResponse() : Ok(Envelope.Ok(createResult.Value));
    }
}