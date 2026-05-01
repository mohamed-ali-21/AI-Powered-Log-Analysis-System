using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogMin.Infrastructure.Abstractions.Intelligence;
using Microsoft.Extensions.Logging;

namespace LogMin.Infrastructure.ExternalServices;

public sealed class IssueAnalysisAgentClient : IIssueAnalysisAgentClient
{
    private static readonly TimeSpan BaseBackoff = TimeSpan.FromMilliseconds(500);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions IssueSerializeOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions ResponseDeserializeOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly IssueAnalysisAgentClientSettings _settings;
    private readonly ILogger<IssueAnalysisAgentClient> _logger;

    public IssueAnalysisAgentClient(
        HttpClient http,
        IssueAnalysisAgentClientSettings settings,
        ILogger<IssueAnalysisAgentClient> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.GetAsync(_settings.ListAppsPath, cancellationToken);
            if (!response.IsSuccessStatusCode) return false;

            var apps = await response.Content.ReadFromJsonAsync<List<string>>(ResponseDeserializeOptions, cancellationToken);
            return apps is not null && apps.Contains(_settings.AppName, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "ADK readiness probe failed at {Path}.", _settings.ListAppsPath);
            return false;
        }
    }

    public async Task<IssueAnalysisAgentResponse> AnalyzeAsync(
        IssueAnalysisAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        var sessionId = request.IssueId;
        await EnsureSessionAsync(sessionId, cancellationToken);

        var userText = JsonSerializer.Serialize(request, IssueSerializeOptions);

        var runBody = new AdkRunRequest(
            AppName: _settings.AppName,
            UserId: _settings.UserId,
            SessionId: sessionId,
            NewMessage: new AdkMessage(
                Role: "user",
                Parts: new[] { new AdkPart(userText) }));

        Exception? lastError = null;

        for (var attempt = 1; attempt <= _settings.MaxRetries; attempt++)
        {
            try
            {
                using var response = await _http.PostAsJsonAsync(_settings.RunPath, runBody, JsonOptions, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var events = await response.Content.ReadFromJsonAsync<List<AdkEvent>>(ResponseDeserializeOptions, cancellationToken)
                                 ?? new List<AdkEvent>();

                    var finalText = ExtractFinalModelText(events)
                        ?? throw new InvalidOperationException("ADK returned no model output for the agent.");

                    var json = StripJsonFence(finalText);
                    var parsed = JsonSerializer.Deserialize<IssueAnalysisAgentResponse>(json, ResponseDeserializeOptions)
                                 ?? throw new InvalidOperationException("Agent JSON deserialized to null.");

                    return parsed with { IssueId = request.IssueId };
                }

                if (!IsTransient(response.StatusCode))
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"ADK returned non-retriable {(int)response.StatusCode} {response.StatusCode}: {body}");
                }

                lastError = new HttpRequestException(
                    $"ADK returned transient {(int)response.StatusCode} {response.StatusCode}");
                _logger.LogWarning(
                    "ADK transient {Status} for issue {IssueId} (attempt {Attempt}/{Max}).",
                    (int)response.StatusCode, request.IssueId, attempt, _settings.MaxRetries);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                lastError = ex;
                _logger.LogWarning(ex,
                    "ADK call failed for issue {IssueId} (attempt {Attempt}/{Max}).",
                    request.IssueId, attempt, _settings.MaxRetries);
            }

            if (attempt < _settings.MaxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(BaseBackoff.TotalMilliseconds * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"ADK call exhausted {_settings.MaxRetries} attempts for issue {request.IssueId}.", lastError);
    }

    private async Task EnsureSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        var path = $"/apps/{Uri.EscapeDataString(_settings.AppName)}" +
                   $"/users/{Uri.EscapeDataString(_settings.UserId)}" +
                   $"/sessions/{Uri.EscapeDataString(sessionId)}";

        using var content = new StringContent("{}", Encoding.UTF8, "application/json");
        using var response = await _http.PostAsync(path, content, cancellationToken);

        if (response.IsSuccessStatusCode) return;

        // ADK returns 400 when the session already exists â€” that's fine for our idempotent flow.
        if (response.StatusCode == HttpStatusCode.BadRequest) return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"ADK session create failed {(int)response.StatusCode} {response.StatusCode}: {body}");
    }

    private static string? ExtractFinalModelText(IEnumerable<AdkEvent> events)
    {
        foreach (var ev in events.Reverse())
        {
            var parts = ev.Content?.Parts;
            if (parts is null) continue;
            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part.Text)) return part.Text;
            }
        }
        return null;
    }

    private static string StripJsonFence(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline > 0) text = text[(firstNewline + 1)..];
            if (text.EndsWith("```")) text = text[..^3];
        }
        return text.Trim();
    }

    private static bool IsTransient(HttpStatusCode status) =>
        (int)status >= 500 || status == HttpStatusCode.RequestTimeout || status == (HttpStatusCode)429;

    private sealed record AdkRunRequest(string AppName, string UserId, string SessionId, AdkMessage NewMessage);

    private sealed record AdkMessage(string Role, IReadOnlyList<AdkPart> Parts);

    private sealed record AdkPart(string Text);

    private sealed class AdkEvent
    {
        public string? Author { get; set; }
        public AdkContent? Content { get; set; }
    }

    private sealed class AdkContent
    {
        public string? Role { get; set; }
        public List<AdkContentPart>? Parts { get; set; }
    }

    private sealed class AdkContentPart
    {
        public string? Text { get; set; }
    }
}

public sealed class IssueAnalysisAgentClientSettings
{
    public string AppName { get; init; } = "logmind_agent";
    public string UserId { get; init; } = "logmind-worker";
    public string ListAppsPath { get; init; } = "/list-apps";
    public string RunPath { get; init; } = "/run";
    public int MaxRetries { get; init; } = 3;
}
