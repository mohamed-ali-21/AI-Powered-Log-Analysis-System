using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Application.DTOs.Common;
using LogMin.Application.DTOs.Issues;
using LogMin.Application.Services.Abstruction;
using LogMin.Infrastructure.Persistence.Entities;

namespace LogMin.Application.Services;

public sealed class IssueQueryService : IIssueQueryService
{
    private const int IssueDetailsSampleSize = 5;

    private readonly IIssueRepository _issues;
    private readonly IIssueAnalysisRepository _analyses;

    public IssueQueryService(IIssueRepository issues, IIssueAnalysisRepository analyses)
    {
        _issues = issues;
        _analyses = analyses;
    }

    public async Task<PagedResult<IssueDto>> SearchAsync(IssuesQuery query, CancellationToken cancellationToken = default)
    {
        var (skip, take, page, pageSize) = PagingHelpers.Normalize(query.Page, query.PageSize);

        var filter = new IssueFilter(
            IsAiProcessed: query.IsAiProcessed,
            ServiceName: NullIfBlank(query.ServiceName),
            Pattern: NullIfBlank(query.Pattern),
            From: query.From,
            To: query.To,
            Skip: skip,
            Take: take);

        var (items, total) = await _issues.QueryAsync(filter, cancellationToken);

        return new PagedResult<IssueDto>
        {
            Items = items.Select(MapSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<IssueDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var issue = await _issues.GetByIdAsync(id, cancellationToken);
        if (issue is null) return null;

        var samples = await _issues.GetSampleLogReferencesAsync(id, IssueDetailsSampleSize, cancellationToken);
        var latestAnalysisId = await _analyses.GetLatestIdByIssueAsync(id, cancellationToken);

        return new IssueDetailsDto
        {
            Id = issue.Id,
            Pattern = issue.Pattern,
            ServiceName = issue.ServiceName,
            FirstSeen = issue.FirstSeen,
            LastSeen = issue.LastSeen,
            Count = issue.Count,
            AvgScore = issue.AvgScore,
            IsAiProcessed = issue.IsAiProcessed,
            RepresentativeLogId = issue.RepresentativeLogId,
            SampleLogs = samples.Select(s => new IssueLogReferenceDto
            {
                Id = s.Id,
                Timestamp = s.Timestamp,
                Message = s.Message
            }).ToList(),
            LatestAnalysisId = latestAnalysisId
        };
    }

    private static IssueDto MapSummary(Issue issue) => new()
    {
        Id = issue.Id,
        Pattern = issue.Pattern,
        ServiceName = issue.ServiceName,
        FirstSeen = issue.FirstSeen,
        LastSeen = issue.LastSeen,
        Count = issue.Count,
        AvgScore = issue.AvgScore,
        IsAiProcessed = issue.IsAiProcessed,
        RepresentativeLogId = issue.RepresentativeLogId
    };

    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
