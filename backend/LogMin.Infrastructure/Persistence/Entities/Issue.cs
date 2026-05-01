namespace LogMin.Infrastructure.Persistence.Entities;

public class Issue
{
    public Guid Id { get; set; }

    public string Pattern { get; set; } = default!;

    public string ServiceName { get; set; } = default!;

    public DateTime FirstSeen { get; set; }

    public DateTime LastSeen { get; set; }

    public int Count { get; set; }

    public double AvgScore { get; set; }

    public Guid RepresentativeLogId { get; set; }

    public bool IsAiProcessed { get; set; }

    public ICollection<Log> Logs { get; set; } = new List<Log>();

    public ICollection<IssueAnalysis> Analyses { get; set; } = new List<IssueAnalysis>();
}
