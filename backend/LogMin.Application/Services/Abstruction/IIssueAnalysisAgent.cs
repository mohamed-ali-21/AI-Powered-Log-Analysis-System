using LogMin.Infrastructure.Persistence.Entities;

namespace LogMin.Application.Services.Abstruction;

public interface IIssueAnalysisAgent
{
    Task<IssueAnalysisResult> AnalyzeAsync(
        Issue issue,
        IReadOnlyList<string> logs,
        CancellationToken cancellationToken = default);
}

public sealed record IssueAnalysisResult(
    string IssueId,
    string RootCause,
    string Impact,
    string Severity,
    string Correlation,
    IReadOnlyList<string> Recommendations,
    string Summary,
    IReadOnlyList<string> Tags);
