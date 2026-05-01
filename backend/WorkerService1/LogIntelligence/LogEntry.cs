namespace LogMin.Worker.LogIntelligence;

public sealed class LogEntry
{
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
