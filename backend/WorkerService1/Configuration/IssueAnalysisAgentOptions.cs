namespace LogMin.Worker.Configuration;

public sealed class IssueAnalysisAgentOptions
{
    public string BaseUrl { get; set; } = "";
    public string AppName { get; set; } = "logmind_agent";
    public string UserId { get; set; } = "logmind-worker";
    public string ListAppsPath { get; set; } = "/list-apps";
    public string RunPath { get; set; } = "/run";
    public int RequestTimeoutSeconds { get; set; } = 90;
    public int MaxRetries { get; set; } = 3;
}
