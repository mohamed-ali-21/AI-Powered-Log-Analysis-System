namespace LogMin.Infrastructure.Abstractions.Intelligence;

public interface IIssueAnalysisAgentClient
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    Task<IssueAnalysisAgentResponse> AnalyzeAsync(
        IssueAnalysisAgentRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record IssueAnalysisAgentRequest(
    string IssueId,
    string Pattern,
    string ServiceName,
    int LogsCount,
    double AvgScore,
    IReadOnlyList<string> SampleLogs,
    string TimeWindow,
    IReadOnlyList<string> RelatedServices);

public sealed record IssueAnalysisAgentResponse(
    string IssueId,
    string RootCause,
    string Impact,
    string Severity,
    IReadOnlyList<string> Recommendations,
    string Summary,
    IReadOnlyList<string> Tags);
