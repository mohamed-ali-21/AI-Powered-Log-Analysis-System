using LogMin.Infrastructure.Abstractions.Intelligence;
using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Infrastructure.Persistence.Entities;

namespace LogMin.Application.Services;

public sealed class IssueGroupingService : IIssueGroupingService
{
    private const string UnknownPattern = "Unknown";
    private static readonly TimeSpan GroupingWindow = TimeSpan.FromHours(24);

    private readonly IIssueRepository _issues;

    public IssueGroupingService(IIssueRepository issues)
    {
        _issues = issues;
    }

    public async Task<Issue?> GroupAsync(Log log, LogAnalysisOutput analysis, CancellationToken cancellationToken = default)
    {
        if (string.Equals(analysis.Pattern, UnknownPattern, StringComparison.Ordinal))
            return null;

        var notSeenBefore = log.Timestamp - GroupingWindow;

        var existing = await _issues.FindOpenAsync(
            analysis.Pattern,
            log.ServiceName,
            notSeenBefore,
            cancellationToken);

        if (existing is null)
        {
            var created = new Issue
            {
                Id = Guid.NewGuid(),
                Pattern = analysis.Pattern,
                ServiceName = log.ServiceName,
                FirstSeen = log.Timestamp,
                LastSeen = log.Timestamp,
                Count = 1,
                AvgScore = analysis.Score,
                RepresentativeLogId = log.Id
            };

            await _issues.AddAsync(created, cancellationToken);
            return created;
        }

        existing.AvgScore = ((existing.AvgScore * existing.Count) + analysis.Score) / (existing.Count + 1);
        existing.Count += 1;
        if (log.Timestamp > existing.LastSeen) existing.LastSeen = log.Timestamp;
        if (log.Timestamp < existing.FirstSeen) existing.FirstSeen = log.Timestamp;

        return existing;
    }
}
