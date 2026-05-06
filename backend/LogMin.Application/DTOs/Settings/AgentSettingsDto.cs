namespace LogMin.Application.DTOs.Settings;

public sealed class AgentSettingsDto
{
    public string Model { get; set; } = "";
    public string ApiKeyMasked { get; set; } = "";
    public bool ApiKeyConfigured { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Source { get; set; } = "";
    public IReadOnlyList<string> AvailableModels { get; set; } = Array.Empty<string>();
}

public sealed class UpdateAgentSettingsRequest
{
    public string Model { get; set; } = "";
    public string? ApiKey { get; set; }
}

public sealed class TestAgentSettingsRequest
{
    public string Model { get; set; } = "";
    public string? ApiKey { get; set; }
}

public sealed class TestAgentSettingsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int? LatencyMs { get; set; }
}
