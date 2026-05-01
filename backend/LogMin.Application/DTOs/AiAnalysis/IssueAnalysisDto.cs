namespace LogMin.Application.DTOs.AiAnalysis;

public sealed class IssueAnalysisDto
{
    public Guid Id { get; init; }
    public Guid IssueId { get; init; }
    public string? RootCause { get; init; }
    public string? Impact { get; init; }
    public string? Severity { get; init; }
    public string? Summary { get; init; }
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public double? AIConfidence { get; init; }
    public DateTime CreatedAt { get; init; }
}
