using System.Text.Json;
using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Application.DTOs.AiAnalysis;
using LogMin.Application.DTOs.Common;
using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;

namespace LogMin.Application.Services;

public sealed class IssueAnalysisQueryService : IIssueAnalysisQueryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IIssueAnalysisRepository _analyses;
    private readonly IIssueRepository _issues;
    private readonly ILogger<IssueAnalysisQueryService> _logger;

    public IssueAnalysisQueryService(
        IIssueAnalysisRepository analyses,
        IIssueRepository issues,
        ILogger<IssueAnalysisQueryService> logger)
    {
        _analyses = analyses;
        _issues = issues;
        _logger = logger;
    }

    public async Task<PagedResult<IssueAnalysisDto>> SearchAsync(AiAnalysisQuery query, CancellationToken cancellationToken = default)
    {
        var (skip, take, page, pageSize) = PagingHelpers.Normalize(query.Page, query.PageSize);

        var filter = new IssueAnalysisFilter(
            Severity: NullIfBlank(query.Severity),
            From: query.From,
            To: query.To,
            Skip: skip,
            Take: take);

        var (items, total) = await _analyses.QueryAsync(filter, cancellationToken);

        return new PagedResult<IssueAnalysisDto>
        {
            Items = items.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<IssueAnalysisDto?> GetByIssueIdAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        var analysis = await _analyses.GetLatestByIssueAsync(issueId, cancellationToken);
        return analysis is null ? null : Map(analysis);
    }

    public async Task<bool> ScheduleRetryAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        var marked = await _issues.MarkForReanalysisAsync(issueId, cancellationToken);
        if (!marked) return false;

        await _issues.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Issue {IssueId} scheduled for AI re-analysis.", issueId);
        return true;
    }

    private static IssueAnalysisDto Map(IssueAnalysis a) => new()
    {
        Id = a.Id,
        IssueId = a.IssueId,
        RootCause = a.RootCause,
        Impact = a.Impact,
        Severity = a.Severity,
        Summary = a.Summary,
        Recommendations = DeserializeArray(a.Recommendation),
        Tags = DeserializeArray(a.Tags),
        AIConfidence = a.AIConfidence,
        CreatedAt = a.CreatedAt
    };

    private static IReadOnlyList<string> DeserializeArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<string>();
        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
            return parsed ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
        catch (JsonException)
        {
            return new[] { json };
        }
    }

    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
