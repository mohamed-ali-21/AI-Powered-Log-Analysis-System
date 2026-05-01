using LogMin.Infrastructure.Persistence.Entities;

namespace LogMin.Infrastructure.Abstractions.Persistence;

public interface IIssueAnalysisRepository
{
    Task AddAsync(IssueAnalysis analysis, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<IssueAnalysis> Items, int Total)> QueryAsync(IssueAnalysisFilter filter, CancellationToken cancellationToken = default);

    Task<IssueAnalysis?> GetLatestByIssueAsync(Guid issueId, CancellationToken cancellationToken = default);

    Task<Guid?> GetLatestIdByIssueAsync(Guid issueId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record IssueAnalysisFilter(
    string? Severity,
    DateTime? From,
    DateTime? To,
    int Skip,
    int Take);
