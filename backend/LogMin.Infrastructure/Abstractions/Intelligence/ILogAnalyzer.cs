namespace LogMin.Infrastructure.Abstractions.Intelligence;

public interface ILogAnalyzer
{
    LogAnalysisOutput Analyze(LogAnalysisInput input);
}

public sealed record LogAnalysisInput(string Message, string? StackTrace);

public sealed record LogAnalysisOutput(
    string Pattern,
    double Score,
    IReadOnlyList<string> Tokens,
    IReadOnlyDictionary<string, bool> Features,
    IReadOnlyList<AnalysisSignal> Signals);

public sealed record AnalysisSignal(string Name, double Weight, string Family);
