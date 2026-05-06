namespace LogMin.Worker.Configuration;

public sealed class IssueAnalysisOptions
{
    public const string SectionName = "IssueAnalysis";

    public int IntervalSeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 20;
    public int DelayBetweenCallsMs { get; set; } = 250;
}
