namespace LogMin.Worker.Intelligence;

public sealed class IssueAnalysisAgentOptions
{
    public const string SectionName = "IssueAnalysis:Agent";

    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public string ApiKey { get; set; } = "";
    public string ApiKeyEnvVar { get; set; } = "LOGMIND_LLM_API_KEY";
    public string AgentName { get; set; } = "LogMindAnalyst";
}
