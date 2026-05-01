using LogMin.Application.DTOs.AiAnalysis;
using LogMin.Application.DTOs.Common;
using LogMin.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogMin.API.Controllers;

[ApiController]
[Route("api/ai-analysis")]
public sealed class AiAnalysisController : ControllerBase
{
    private readonly IIssueAnalysisQueryService _analyses;

    public AiAnalysisController(IIssueAnalysisQueryService analyses)
    {
        _analyses = analyses;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IssueAnalysisDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<IssueAnalysisDto>>> Search(
        [FromQuery] AiAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _analyses.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{issueId:guid}")]
    [ProducesResponseType(typeof(IssueAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IssueAnalysisDto>> GetByIssueId(Guid issueId, CancellationToken cancellationToken)
    {
        var analysis = await _analyses.GetByIssueIdAsync(issueId, cancellationToken);
        return analysis is null ? NotFound() : Ok(analysis);
    }

    [HttpPost("retry/{issueId:guid}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Retry(Guid issueId, CancellationToken cancellationToken)
    {
        var scheduled = await _analyses.ScheduleRetryAsync(issueId, cancellationToken);
        if (!scheduled) return NotFound();

        return Accepted(new
        {
            issueId,
            status = "scheduled",
            message = "Issue marked for re-analysis; the worker will pick it up on the next cycle."
        });
    }
}
