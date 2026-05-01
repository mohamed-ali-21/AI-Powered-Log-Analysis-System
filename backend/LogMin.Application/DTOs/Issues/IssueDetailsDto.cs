namespace LogMin.Application.DTOs.Issues;

public sealed class IssueDetailsDto
{
    public Guid Id { get; init; }
    public string Pattern { get; init; } = "";
    public string ServiceName { get; init; } = "";
    public DateTime FirstSeen { get; init; }
    public DateTime LastSeen { get; init; }
    public int Count { get; init; }
    public double AvgScore { get; init; }
    public bool IsAiProcessed { get; init; }
    public Guid RepresentativeLogId { get; init; }
    public IReadOnlyList<IssueLogReferenceDto> SampleLogs { get; init; } = Array.Empty<IssueLogReferenceDto>();
    public Guid? LatestAnalysisId { get; init; }
}

public sealed class IssueLogReferenceDto
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string Message { get; init; } = "";
}
