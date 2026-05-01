using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Infrastructure.Persistence.Entities;
using LogMin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogMin.Infrastructure.Repositories;

public sealed class IssueRepository : IIssueRepository
{
    private readonly AppDbContext _db;

    public IssueRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Issue?> FindOpenAsync(
        string pattern,
        string serviceName,
        DateTime notSeenBefore,
        CancellationToken cancellationToken = default)
    {
        return _db.Issues
            .Where(x => x.Pattern == pattern
                        && x.ServiceName == serviceName
                        && x.LastSeen >= notSeenBefore)
            .OrderByDescending(x => x.LastSeen)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(Issue issue, CancellationToken cancellationToken = default)
    {
        return _db.Issues.AddAsync(issue, cancellationToken).AsTask();
    }

    public Task<List<Issue>> GetPendingAiAnalysisAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return _db.Issues
            .Where(x => !x.IsAiProcessed)
            .OrderBy(x => x.LastSeen)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task<List<string>> GetRecentLogMessagesAsync(Guid issueId, int max, CancellationToken cancellationToken = default)
    {
        return _db.Logs
            .Where(x => x.IssueId == issueId)
            .OrderByDescending(x => x.Timestamp)
            .Take(max)
            .Select(x => x.Message)
            .ToListAsync(cancellationToken);
    }

    public Task<Issue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Issues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Issue> Items, int Total)> QueryAsync(IssueFilter filter, CancellationToken cancellationToken = default)
    {
        var q = _db.Issues.AsNoTracking().AsQueryable();

        if (filter.IsAiProcessed.HasValue)
            q = q.Where(x => x.IsAiProcessed == filter.IsAiProcessed.Value);

        if (!string.IsNullOrEmpty(filter.ServiceName))
            q = q.Where(x => x.ServiceName == filter.ServiceName);

        if (!string.IsNullOrEmpty(filter.Pattern))
            q = q.Where(x => x.Pattern == filter.Pattern);

        if (filter.From.HasValue)
            q = q.Where(x => x.LastSeen >= filter.From.Value);

        if (filter.To.HasValue)
            q = q.Where(x => x.LastSeen <= filter.To.Value);

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderByDescending(x => x.LastSeen)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<List<IssueLogReference>> GetSampleLogReferencesAsync(Guid issueId, int max, CancellationToken cancellationToken = default)
    {
        return await _db.Logs
            .AsNoTracking()
            .Where(x => x.IssueId == issueId)
            .OrderByDescending(x => x.Timestamp)
            .Take(max)
            .Select(x => new IssueLogReference(x.Id, x.Timestamp, x.Message))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> MarkForReanalysisAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        var issue = await _db.Issues.FirstOrDefaultAsync(x => x.Id == issueId, cancellationToken);
        if (issue is null) return false;

        issue.IsAiProcessed = false;
        return true;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
