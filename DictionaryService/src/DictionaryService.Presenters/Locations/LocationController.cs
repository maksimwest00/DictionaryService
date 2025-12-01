using DictionaryService.Application.Locations;
using DictionaryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryService.Presenters.Locations;

[ApiController]
[Route("/api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationSerivce _locationService;

    public LocationController(ILocationSerivce locationSerivce) => _locationService = locationSerivce;

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateLocationDto request,
        CancellationToken cancellationToken)
    {
        Guid locationId = await _locationService.CreateAsync(request, cancellationToken);
        return Ok(locationId);
    }
}