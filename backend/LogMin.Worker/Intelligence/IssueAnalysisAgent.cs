using System.Text.Json;
using LogMin.Application.Services;
using LogMin.Application.Services.Abstruction;
using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace LogMin.Worker.Intelligence;

public sealed class IssueAnalysisAgent : IIssueAnalysisAgent
{
    private static readonly HashSet<string> AllowedSeverities =
        new(StringComparer.OrdinalIgnoreCase) { "Low", "Medium", "High", "Critical" };

    private static readonly JsonSerializerOptions InputJson = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions OutputJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IAgentSettingsService _settings;
    private readonly ILogger<IssueAnalysisAgent> _logger;

    public IssueAnalysisAgent(IAgentSettingsService settings, ILogger<IssueAnalysisAgent> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<IssueAnalysisResult> AnalyzeAsync(
        Issue issue,
        IReadOnlyList<string> logs,
        CancellationToken cancellationToken = default)
    {
        var resolved = await _settings.ResolveAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(resolved.ApiKey))
            throw new InvalidOperationException(
                "LLM API key not configured. Set it via the Settings page or appsettings.json / LOGMIND_LLM_API_KEY env var.");
        if (string.IsNullOrWhiteSpace(resolved.Model))
            throw new InvalidOperationException("LLM Model not configured.");

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: resolved.Model,
            endpoint: new Uri(GroqDefaults.ApiBase),
            apiKey: resolved.ApiKey,
            serviceId: GroqDefaults.AgentServiceId);

        var kernel = builder.Build();
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.2,
            ResponseFormat = "json_object"
        };

        var input = new AgentInput(
            IssueId: issue.Id.ToString(),
            Pattern: issue.Pattern,
            ServiceName: issue.ServiceName,
            LogsCount: issue.Count,
            AvgScore: issue.AvgScore,
            SampleLogs: logs,
            TimeWindow: $"{issue.FirstSeen:O}..{issue.LastSeen:O}",
            RelatedServices: Array.Empty<string>());

        var userText = JsonSerializer.Serialize(input, InputJson);

        var history = new ChatHistory();
        history.AddSystemMessage(LogMindAnalystPrompt.SystemPrompt);
        history.AddUserMessage(userText);

        var reply = await chat.GetChatMessageContentAsync(history, executionSettings, kernel, cancellationToken);

        var rawText = reply.Content;
        if (string.IsNullOrWhiteSpace(rawText))
            throw new InvalidOperationException($"Agent returned empty output for issue {issue.Id}.");

        var json = StripJsonFence(rawText);

        AgentOutput? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<AgentOutput>(json, OutputJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Agent output for issue {issue.Id} was not valid JSON: {Truncate(rawText)}", ex);
        }

        if (parsed is null)
            throw new InvalidOperationException($"Agent JSON for issue {issue.Id} deserialized to null.");

        var severity = parsed.Severity ?? "";
        if (!AllowedSeverities.Contains(severity))
            throw new InvalidOperationException(
                $"Agent returned invalid severity '{severity}' for issue {issue.Id}.");

        return new IssueAnalysisResult(
            IssueId: issue.Id.ToString(),
            RootCause: parsed.RootCause ?? "",
            Impact: parsed.Impact ?? "",
            Severity: NormalizeSeverity(severity),
            Correlation: parsed.Correlation ?? "",
            Recommendations: parsed.Recommendations ?? new List<string>(),
            Summary: parsed.Summary ?? "",
            Tags: parsed.Tags ?? new List<string>());
    }

    private static string StripJsonFence(string text)
    {
        text = text.Trim();
        if (!text.StartsWith("```")) return text;

        var firstNewline = text.IndexOf('\n');
        if (firstNewline > 0) text = text[(firstNewline + 1)..];
        if (text.EndsWith("```")) text = text[..^3];
        return text.Trim();
    }

    private static string NormalizeSeverity(string value) =>
        char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();

    private static string Truncate(string s) =>
        s.Length <= 500 ? s : s[..500] + "...";

    private sealed record AgentInput(
        string IssueId,
        string Pattern,
        string ServiceName,
        int LogsCount,
        double AvgScore,
        IReadOnlyList<string> SampleLogs,
        string TimeWindow,
        IReadOnlyList<string> RelatedServices);

    private sealed class AgentOutput
    {
        public string? IssueId { get; set; }
        public string? RootCause { get; set; }
        public string? Impact { get; set; }
        public string? Severity { get; set; }
        public string? Correlation { get; set; }
        public List<string>? Recommendations { get; set; }
        public string? Summary { get; set; }
        public List<string>? Tags { get; set; }
    }
}
