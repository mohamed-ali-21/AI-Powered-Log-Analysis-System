namespace LogMin.Infrastructure.Persistence.Entities;

public class Log
{
    public Guid Id { get; set; }

    public string Message { get; set; } = default!;

    public string? StackTrace { get; set; }

    public string ServiceName { get; set; } = default!;

    public DateTime Timestamp { get; set; }

    public bool Processed { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? Pattern { get; set; }

    public double? Score { get; set; }

    public Guid? IssueId { get; set; }
    public Issue? Issue { get; set; }
}
