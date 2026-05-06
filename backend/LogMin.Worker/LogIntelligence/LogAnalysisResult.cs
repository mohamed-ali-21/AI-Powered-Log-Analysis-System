using System.Text.Json.Serialization;

namespace LogMin.Worker.LogIntelligence;

public sealed class SignalHit
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    [JsonIgnore]
    public string Family { get; set; } = string.Empty;
}

public sealed class LogAnalysisResult
{
    [JsonPropertyName("tokens")]
    public List<string> Tokens { get; set; } = new();

    [JsonPropertyName("signals")]
    public List<SignalHit> Signals { get; set; } = new();

    [JsonPropertyName("features")]
    public Dictionary<string, bool> Features { get; set; } = new();

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("pattern")]
    public string Pattern { get; set; } = "Unknown";
}
