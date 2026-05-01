using LogMin.Infrastructure.Abstractions.Intelligence;

namespace LogMin.Worker.LogIntelligence;

public sealed class LogIntelligenceAnalyzer : ILogAnalyzer
{
    private readonly LogIntelligenceWorker _worker;

    public LogIntelligenceAnalyzer(LogIntelligenceWorker worker)
    {
        _worker = worker;
    }

    public LogAnalysisOutput Analyze(LogAnalysisInput input)
    {
        var entry = new LogEntry
        {
            Message = input.Message,
            StackTrace = input.StackTrace
        };

        var result = _worker.Analyze(entry);

        var signals = result.Signals
            .Select(s => new AnalysisSignal(s.Name, s.Weight, s.Family))
            .ToList();

        return new LogAnalysisOutput(
            result.Pattern,
            result.Score,
            result.Tokens,
            result.Features,
            signals);
    }
}
