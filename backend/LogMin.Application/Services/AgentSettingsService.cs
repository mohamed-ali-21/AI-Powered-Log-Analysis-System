using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogMin.Application.DTOs.Settings;
using LogMin.Application.Services.Abstruction;
using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Configuration;

namespace LogMin.Application.Services;

public sealed class AgentSettingsService : IAgentSettingsService
{
    private const string ConfigSection = "IssueAnalysis:Agent";

    private readonly IAgentSettingRepository _repo;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpFactory;

    public AgentSettingsService(IAgentSettingRepository repo, IConfiguration config, IHttpClientFactory httpFactory)
    {
        _repo = repo;
        _config = config;
        _httpFactory = httpFactory;
    }

    public async Task<AgentSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var resolved = await ResolveAsync(cancellationToken);
        var stored = await _repo.GetAsync(cancellationToken);

        var model = NormalizeModel(resolved.Model);

        return new AgentSettingsDto
        {
            Model = model,
            ApiKeyMasked = MaskKey(resolved.ApiKey),
            ApiKeyConfigured = !string.IsNullOrWhiteSpace(resolved.ApiKey),
            UpdatedAt = stored?.UpdatedAt,
            Source = resolved.Source,
            AvailableModels = GroqDefaults.AvailableModels
        };
    }

    public async Task<AgentSettingsDto> UpdateAsync(UpdateAgentSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var model = ValidateModel(request.Model);

        var existing = await _repo.GetAsync(cancellationToken);
        var keyToUse = string.IsNullOrWhiteSpace(request.ApiKey)
            ? (existing?.ApiKey ?? ResolveApiKeyFromConfig() ?? "")
            : request.ApiKey.Trim();

        if (string.IsNullOrWhiteSpace(keyToUse))
            throw new InvalidOperationException("An API key is required (no value provided and no fallback configured).");

        await _repo.UpsertAsync(new AgentSetting
        {
            Model = model,
            ApiKey = keyToUse,
            ApiServerUrl = existing?.ApiServerUrl ?? ""
        }, cancellationToken);

        await _repo.SaveChangesAsync(cancellationToken);

        return await GetAsync(cancellationToken);
    }

    public async Task<TestAgentSettingsResponse> TestAsync(TestAgentSettingsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = ValidateModel(request.Model);

            var key = request.ApiKey;
            if (string.IsNullOrWhiteSpace(key))
            {
                var existing = await _repo.GetAsync(cancellationToken);
                key = existing?.ApiKey ?? ResolveApiKeyFromConfig();
            }

            if (string.IsNullOrWhiteSpace(key))
                return new TestAgentSettingsResponse { Success = false, Message = "API key is empty." };

            using var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(15);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            var payload = new
            {
                model,
                messages = new[] { new { role = "user", content = "Reply with the single word: ok" } },
                max_tokens = 5,
                temperature = 0
            };

            var sw = Stopwatch.StartNew();
            using var response = await http.PostAsJsonAsync(GroqDefaults.ChatCompletionsUrl, payload, cancellationToken);
            sw.Stop();

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new TestAgentSettingsResponse
                {
                    Success = false,
                    Message = $"HTTP {(int)response.StatusCode} {response.StatusCode}: {Truncate(body, 200)}",
                    LatencyMs = (int)sw.ElapsedMilliseconds
                };
            }

            return new TestAgentSettingsResponse
            {
                Success = true,
                Message = $"Connection OK · HTTP {(int)response.StatusCode}",
                LatencyMs = (int)sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            return new TestAgentSettingsResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<ResolvedAgentSettings> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var stored = await _repo.GetAsync(cancellationToken);
        if (stored is not null && !string.IsNullOrWhiteSpace(stored.ApiKey))
        {
            return new ResolvedAgentSettings(
                Model: NormalizeModel(stored.Model),
                ApiKey: stored.ApiKey,
                ApiServerUrl: stored.ApiServerUrl ?? "",
                Source: "database");
        }

        var section = _config.GetSection(ConfigSection);
        var fallbackKey = ResolveApiKeyFromConfig() ?? "";

        return new ResolvedAgentSettings(
            Model: NormalizeModel(section["Model"] ?? GroqDefaults.DefaultModel),
            ApiKey: fallbackKey,
            ApiServerUrl: "",
            Source: "appsettings");
    }

    private static string ValidateModel(string? model)
    {
        var trimmed = (model ?? "").Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Model is required.");
        if (!GroqDefaults.AvailableModels.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Model '{trimmed}' is not in the supported list. Allowed: {string.Join(", ", GroqDefaults.AvailableModels)}.");
        return trimmed;
    }

    private static string NormalizeModel(string? model)
    {
        var trimmed = (model ?? "").Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return GroqDefaults.DefaultModel;
        var match = GroqDefaults.AvailableModels.FirstOrDefault(m => m.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        return match ?? GroqDefaults.DefaultModel;
    }

    private string? ResolveApiKeyFromConfig()
    {
        var section = _config.GetSection(ConfigSection);
        var direct = section["ApiKey"];
        if (!string.IsNullOrWhiteSpace(direct)) return direct;

        var envVarName = section["ApiKeyEnvVar"];
        if (string.IsNullOrWhiteSpace(envVarName)) envVarName = "LOGMIND_LLM_API_KEY";
        var fromEnv = Environment.GetEnvironmentVariable(envVarName);
        return string.IsNullOrWhiteSpace(fromEnv) ? null : fromEnv;
    }

    private static string MaskKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (key.Length <= 8) return new string('•', key.Length);
        return key[..4] + new string('•', Math.Min(8, key.Length - 8)) + key[^4..];
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "...";
}
