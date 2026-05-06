using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Application.DTOs.Common;
using LogMin.Application.DTOs.Logs;
using LogMin.Infrastructure.Persistence.Entities;
using LogMin.Application.Services.Abstruction;

namespace LogMin.Application.Services;

public sealed class LogQueryService : ILogQueryService
{
    private readonly ILogRepository _logs;

    public LogQueryService(ILogRepository logs)
    {
        _logs = logs;
    }

    public async Task<PagedResult<LogDto>> SearchAsync(LogsQuery query, CancellationToken cancellationToken = default)
    {
        var (skip, take, page, pageSize) = PagingHelpers.Normalize(query.Page, query.PageSize);

        var filter = new LogFilter(
            ServiceName: NullIfBlank(query.ServiceName),
            Pattern: NullIfBlank(query.Pattern),
            IssueId: query.IssueId,
            From: query.From,
            To: query.To,
            Skip: skip,
            Take: take);

        var (items, total) = await _logs.QueryAsync(filter, cancellationToken);

        return new PagedResult<LogDto>
        {
            Items = items.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<LogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _logs.GetByIdAsync(id, cancellationToken);
        return log is null ? null : Map(log);
    }

    private static LogDto Map(Log log) => new()
    {
        Id = log.Id,
        Message = log.Message,
        StackTrace = log.StackTrace,
        ServiceName = log.ServiceName,
        Timestamp = log.Timestamp,
        Processed = log.Processed,
        ProcessedAt = log.ProcessedAt,
        Pattern = log.Pattern,
        Score = log.Score,
        IssueId = log.IssueId
    };

    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
