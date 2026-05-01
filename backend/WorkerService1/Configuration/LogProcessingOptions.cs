namespace LogMin.Worker.Configuration;

public sealed class LogProcessingOptions
{
    public const string SectionName = "LogProcessing";

    public int IntervalSeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 100;
}
