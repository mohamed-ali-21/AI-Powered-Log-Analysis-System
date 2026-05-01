using LogMin.Application.DTOs;
using LogMin.Application.DTOs.Common;
using LogMin.Application.DTOs.Logs;
using LogMin.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogMin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LogsController : ControllerBase
{
    private readonly ILogIngestionService _ingestion;
    private readonly ILogQueryService _logs;

    public LogsController(ILogIngestionService ingestion, ILogQueryService logs)
    {
        _ingestion = ingestion;
        _logs = logs;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IngestLogResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IngestLogResponse>> Ingest(
        [FromBody] IngestLogRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ingestion.IngestAsync(request, cancellationToken);
        return Accepted(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LogDto>>> Search(
        [FromQuery] LogsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _logs.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LogDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var log = await _logs.GetByIdAsync(id, cancellationToken);
        return log is null ? NotFound() : Ok(log);
    }
}
