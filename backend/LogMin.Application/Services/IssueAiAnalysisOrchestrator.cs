using System.Text.Json;
using LogMin.Infrastructure.Abstractions.Intelligence;
using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;

namespace LogMin.Application.Services;

public sealed class IssueAiAnalysisOrchestrator : IIssueAiAnalysisOrchestrator
{
    private const int SampleLogCount = 5;

    private readonly IIssueRepository _issues;
    private readonly IIssueAnalysisRepository _analyses;
    private readonly IIssueAnalysisAgentClient _agent;
    private readonly ILogger<IssueAiAnalysisOrchestrator> _logger;

    public IssueAiAnalysisOrchestrator(
        IIssueRepository issues,
        IIssueAnalysisRepository analyses,
        IIssueAnalysisAgentClient agent,
        ILogger<IssueAiAnalysisOrchestrator> logger)
    {
        _issues = issues;
        _analyses = analyses;
        _agent = agent;
        _logger = logger;
    }

    public async Task<int> ProcessBatchAsync(int batchSize, int delayBetweenCallsMs, CancellationToken cancellationToken = default)
    {
        var pending = await _issues.GetPendingAiAnalysisAsync(batchSize, cancellationToken);
        if (pending.Count == 0) return 0;

        if (!await _agent.IsHealthyAsync(cancellationToken))
        {
            _logger.LogWarning("Agent health probe failed; skipping cycle ({Pending} issue(s) remain pending).", pending.Count);
            return 0;
        }

        var processed = 0;
        for (var i = 0; i < pending.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var issue = pending[i];
            try
            {
                var sampleLogs = await _issues.GetRecentLogMessagesAsync(issue.Id, SampleLogCount, cancellationToken);

                var request = new IssueAnalysisAgentRequest(
                    IssueId: issue.Id.ToString(),
                    Pattern: issue.Pattern,
                    ServiceName: issue.ServiceName,
                    LogsCount: issue.Count,
                    AvgScore: issue.AvgScore,
                    SampleLogs: sampleLogs,
                    TimeWindow: $"{issue.FirstSeen:O}..{issue.LastSeen:O}",
                    RelatedServices: Array.Empty<string>());

                var response = await _agent.AnalyzeAsync(request, cancellationToken);

                var analysis = new IssueAnalysis
                {
                    Id = Guid.NewGuid(),
                    IssueId = issue.Id,
                    RootCause = response.RootCause,
                    Impact = response.Impact,
                    Severity = response.Severity,
                    Recommendation = JsonSerializer.Serialize(response.Recommendations),
                    Summary = response.Summary,
                    Tags = JsonSerializer.Serialize(response.Tags),
                    CreatedAt = DateTime.UtcNow
                };

                await _analyses.AddAsync(analysis, cancellationToken);
                issue.IsAiProcessed = true;

                await _analyses.SaveChangesAsync(cancellationToken);

                processed++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI analysis failed for issue {IssueId}; will retry next cycle.", issue.Id);
            }

            if (delayBetweenCallsMs > 0 && i < pending.Count - 1)
            {
                try { await Task.Delay(delayBetweenCallsMs, cancellationToken); }
                catch (OperationCanceledException) { throw; }
            }
        }

        return processed;
    }
}
