using LogMin.Application.Services;
using LogMin.Worker.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogMin.Worker.BackgroundServices;

public sealed class IssueAnalysisBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<IssueAnalysisOptions> _options;
    private readonly ILogger<IssueAnalysisBackgroundService> _logger;

    public IssueAnalysisBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<IssueAnalysisOptions> options,
        ILogger<IssueAnalysisBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IssueAnalysisBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var opts = _options.CurrentValue;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IIssueAiAnalysisOrchestrator>();

                var processed = await orchestrator.ProcessBatchAsync(
                    opts.BatchSize,
                    opts.DelayBetweenCallsMs,
                    stoppingToken);

                if (processed > 0)
                    _logger.LogInformation("AI-analyzed {Count} issue(s) in this cycle.", processed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during issue AI analysis cycle.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(opts.IntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("IssueAnalysisBackgroundService stopping.");
    }
}
