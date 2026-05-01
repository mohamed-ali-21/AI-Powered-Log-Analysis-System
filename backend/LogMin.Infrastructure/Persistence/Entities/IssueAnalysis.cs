namespace LogMin.Infrastructure.Persistence.Entities;

public class IssueAnalysis
{
    public Guid Id { get; set; }

    public Guid IssueId { get; set; }
    public Issue Issue { get; set; } = default!;

    public string? RootCause { get; set; }

    public string? Impact { get; set; }

    public string? Recommendation { get; set; }

    public string? Severity { get; set; }

    public string? Summary { get; set; }

    public string? Tags { get; set; }

    public double? AIConfidence { get; set; }

    public DateTime CreatedAt { get; set; }
}
