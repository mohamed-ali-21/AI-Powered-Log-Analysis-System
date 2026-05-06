namespace LogMin.Application.Services.Abstruction;

public interface IIssueAiAnalysisOrchestrator
{
    Task<int> ProcessBatchAsync(int batchSize, int delayBetweenCallsMs, CancellationToken cancellationToken = default);
}
