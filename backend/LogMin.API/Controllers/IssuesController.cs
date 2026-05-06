using LogMin.Application.DTOs.Common;
using LogMin.Application.DTOs.Issues;
using LogMin.Application.Services.Abstruction;
using Microsoft.AspNetCore.Mvc;

namespace LogMin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IssuesController : ControllerBase
{
    private readonly IIssueQueryService _issues;

    public IssuesController(IIssueQueryService issues)
    {
        _issues = issues;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IssueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<IssueDto>>> Search(
        [FromQuery] IssuesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _issues.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IssueDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IssueDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var issue = await _issues.GetByIdAsync(id, cancellationToken);
        return issue is null ? NotFound() : Ok(issue);
    }
}
