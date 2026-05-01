namespace LogMin.Worker.Configuration;

public sealed class IssueAnalysisOptions
{
    public const string SectionName = "IssueAnalysis";

    public int IntervalSeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 20;
    public int DelayBetweenCallsMs { get; set; } = 250;
    public bool HealthCheckEnabled { get; set; } = true;

    public string DefaultAgent { get; set; } = "logmind";
    public Dictionary<string, IssueAnalysisAgentOptions> Agents { get; set; } = new();

    public IssueAnalysisAgentOptions GetDefaultAgent()
    {
        if (!Agents.TryGetValue(DefaultAgent, out var agent))
            throw new InvalidOperationException(
                $"IssueAnalysis:Agents:{DefaultAgent} is not configured.");
        if (string.IsNullOrWhiteSpace(agent.BaseUrl))
            throw new InvalidOperationException(
                $"IssueAnalysis:Agents:{DefaultAgent}:BaseUrl is empty.");
        return agent;
    }
}
