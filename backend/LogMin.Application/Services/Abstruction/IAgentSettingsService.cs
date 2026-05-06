using LogMin.Application.DTOs.Settings;

namespace LogMin.Application.Services.Abstruction;

public interface IAgentSettingsService
{
    Task<AgentSettingsDto> GetAsync(CancellationToken cancellationToken = default);
    Task<AgentSettingsDto> UpdateAsync(UpdateAgentSettingsRequest request, CancellationToken cancellationToken = default);
    Task<TestAgentSettingsResponse> TestAsync(TestAgentSettingsRequest request, CancellationToken cancellationToken = default);
    Task<ResolvedAgentSettings> ResolveAsync(CancellationToken cancellationToken = default);
}

public sealed record ResolvedAgentSettings(
    string Model,
    string ApiKey,
    string ApiServerUrl,
    string Source);
