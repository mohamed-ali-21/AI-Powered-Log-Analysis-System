using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Infrastructure.Persistence.Entities;
using LogMin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogMin.Infrastructure.Repositories;

public sealed class IssueAnalysisRepository : IIssueAnalysisRepository
{
    private readonly AppDbContext _db;

    public IssueAnalysisRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(IssueAnalysis analysis, CancellationToken cancellationToken = default)
    {
        return _db.IssueAnalyses.AddAsync(analysis, cancellationToken).AsTask();
    }

    public async Task<(IReadOnlyList<IssueAnalysis> Items, int Total)> QueryAsync(IssueAnalysisFilter filter, CancellationToken cancellationToken = default)
    {
        var q = _db.IssueAnalyses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(filter.Severity))
            q = q.Where(x => x.Severity == filter.Severity);

        if (filter.From.HasValue)
            q = q.Where(x => x.CreatedAt >= filter.From.Value);

        if (filter.To.HasValue)
            q = q.Where(x => x.CreatedAt <= filter.To.Value);

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<IssueAnalysis?> GetLatestByIssueAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        return _db.IssueAnalyses
            .AsNoTracking()
            .Where(x => x.IssueId == issueId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetLatestIdByIssueAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        var ids = await _db.IssueAnalyses
            .AsNoTracking()
            .Where(x => x.IssueId == issueId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => (Guid?)x.Id)
            .Take(1)
            .ToListAsync(cancellationToken);

        return ids.FirstOrDefault();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
