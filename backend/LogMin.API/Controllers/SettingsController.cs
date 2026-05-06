using LogMin.Application.DTOs.Settings;
using LogMin.Application.Services.Abstruction;
using Microsoft.AspNetCore.Mvc;

namespace LogMin.API.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly IAgentSettingsService _settings;

    public SettingsController(IAgentSettingsService settings)
    {
        _settings = settings;
    }

    [HttpGet("agent")]
    [ProducesResponseType(typeof(AgentSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentSettingsDto>> GetAgent(CancellationToken cancellationToken)
    {
        var dto = await _settings.GetAsync(cancellationToken);
        return Ok(dto);
    }

    [HttpPut("agent")]
    [ProducesResponseType(typeof(AgentSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentSettingsDto>> UpdateAgent(
        [FromBody] UpdateAgentSettingsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _settings.UpdateAsync(request, cancellationToken);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid settings", Detail = ex.Message });
        }
    }

    [HttpPost("agent/test")]
    [ProducesResponseType(typeof(TestAgentSettingsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TestAgentSettingsResponse>> TestAgent(
        [FromBody] TestAgentSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _settings.TestAsync(request, cancellationToken);
        return Ok(result);
    }
}
