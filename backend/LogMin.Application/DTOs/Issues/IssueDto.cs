namespace LogMin.Application.DTOs.Issues;

public sealed class IssueDto
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
}
