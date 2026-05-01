namespace LogMin.Application.DTOs.Issues;

public sealed class IssuesQuery
{
    public bool? IsAiProcessed { get; set; }
    public string? ServiceName { get; set; }
    public string? Pattern { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
