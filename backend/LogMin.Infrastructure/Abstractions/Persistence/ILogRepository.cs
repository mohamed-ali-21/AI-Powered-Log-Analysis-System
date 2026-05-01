using LogMin.Infrastructure.Persistence.Entities;

namespace LogMin.Infrastructure.Abstractions.Persistence;

public interface ILogRepository
{
    Task AddAsync(Log log, CancellationToken cancellationToken = default);

    Task<List<Log>> GetUnprocessedBatchAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<Log?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Log> Items, int Total)> QueryAsync(LogFilter filter, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record LogFilter(
    string? ServiceName,
    string? Pattern,
    DateTime? From,
    DateTime? To,
    int Skip,
    int Take);
