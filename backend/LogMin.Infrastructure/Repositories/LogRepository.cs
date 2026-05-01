using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Infrastructure.Persistence.Entities;
using LogMin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogMin.Infrastructure.Repositories;

public sealed class LogRepository : ILogRepository
{
    private readonly AppDbContext _db;

    public LogRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Log log, CancellationToken cancellationToken = default)
    {
        return _db.Logs.AddAsync(log, cancellationToken).AsTask();
    }

    public Task<List<Log>> GetUnprocessedBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return _db.Logs
            .Where(x => !x.Processed)
            .OrderBy(x => x.Timestamp)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task<Log?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Logs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Log> Items, int Total)> QueryAsync(LogFilter filter, CancellationToken cancellationToken = default)
    {
        var q = _db.Logs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(filter.ServiceName))
            q = q.Where(x => x.ServiceName == filter.ServiceName);

        if (!string.IsNullOrEmpty(filter.Pattern))
            q = q.Where(x => x.Pattern == filter.Pattern);

        if (filter.From.HasValue)
            q = q.Where(x => x.Timestamp >= filter.From.Value);

        if (filter.To.HasValue)
            q = q.Where(x => x.Timestamp <= filter.To.Value);

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderByDescending(x => x.Timestamp)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
