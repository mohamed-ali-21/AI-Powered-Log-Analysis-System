namespace LogMin.Application.Services;

public interface IIssueAiAnalysisOrchestrator
{
    Task<int> ProcessBatchAsync(int batchSize, int delayBetweenCallsMs, CancellationToken cancellationToken = default);
}
