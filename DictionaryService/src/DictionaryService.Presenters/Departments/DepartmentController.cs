using CSharpFunctionalExtensions;
using DictionaryService.Application.Abstractions;
using DictionaryService.Application.Departments.CreateDepartment;
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
}