using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Departments.CreateDepartment;
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
    public async Task<IActionResult> UpdateLocationAsync(
        [FromRoute] Guid departmentId,
        [FromServices] ILogger<DepartmentController> logger,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(Ok());
    }
}