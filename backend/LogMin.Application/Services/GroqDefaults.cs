namespace LogMin.Application.Services;

public static class GroqDefaults
{
    public const string ChatCompletionsUrl = "https://api.groq.com/openai/v1/chat/completions";

    public const string ApiBase = "https://api.groq.com/openai/v1";

    public const string DefaultModel = "llama-3.3-70b-versatile";

    public const string AgentServiceId = "LogMindAnalyst";

    public static readonly IReadOnlyList<string> AvailableModels = new[]
    {
        "llama-3.3-70b-versatile",
        "llama-3.1-8b-instant",
        "llama3-70b-8192",
        "llama3-8b-8192",
        "mixtral-8x7b-32768",
        "gemma2-9b-it",
        "deepseek-r1-distill-llama-70b",
        "qwen-2.5-32b"
    };
}
