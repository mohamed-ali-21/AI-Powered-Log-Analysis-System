using LogMin.Infrastructure.Persistence.Entities;

namespace LogMin.Infrastructure.Abstractions.Persistence;

public interface IIssueRepository
{
    Task<Issue?> FindOpenAsync(
        string pattern,
        string serviceName,
        DateTime notSeenBefore,
        CancellationToken cancellationToken = default);

    Task AddAsync(Issue issue, CancellationToken cancellationToken = default);

    Task<List<Issue>> GetPendingAiAnalysisAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<List<string>> GetRecentLogMessagesAsync(Guid issueId, int max, CancellationToken cancellationToken = default);

    Task<Issue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Issue> Items, int Total)> QueryAsync(IssueFilter filter, CancellationToken cancellationToken = default);

    Task<List<IssueLogReference>> GetSampleLogReferencesAsync(Guid issueId, int max, CancellationToken cancellationToken = default);

    Task<bool> MarkForReanalysisAsync(Guid issueId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record IssueFilter(
    bool? IsAiProcessed,
    string? ServiceName,
    string? Pattern,
    DateTime? From,
    DateTime? To,
    int Skip,
    int Take);

public sealed record IssueLogReference(Guid Id, DateTime Timestamp, string Message);
