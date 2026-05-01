namespace LogMin.Application.DTOs.AiAnalysis;

public sealed class AiAnalysisQuery
{
    public string? Severity { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
