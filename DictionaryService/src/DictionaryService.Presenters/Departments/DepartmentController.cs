using DictionaryService.Contracts.Departments;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryService.Presenters.Departments;

[ApiController]
[Route("[controller]")]
public class DepartmentController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentDto request,
        CancellationToken cancellationToken) =>
        await Task.FromResult(Ok("Department created"));

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] GetDepartmentDto dto,
        CancellationToken cancellationToken) =>
        await Task.FromResult(Ok("Department get"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        await Task.FromResult(Ok("Department get by id"));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDepartmentDto request,
        CancellationToken cancellationToken) =>
        await Task.FromResult(Ok("Department updated"));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        await Task.FromResult(Ok("Department deleted"));
}