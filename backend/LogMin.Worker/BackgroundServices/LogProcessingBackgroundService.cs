using LogMin.Infrastructure.Abstractions.Intelligence;
using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Application.Services;
using LogMin.Application.Services.Abstruction;
using LogMin.Infrastructure.Persistence.Entities;
using LogMin.Worker.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogMin.Worker.BackgroundServices;

public sealed class LogProcessingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<LogProcessingOptions> _options;
    private readonly ILogger<LogProcessingBackgroundService> _logger;

    public LogProcessingBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<LogProcessingOptions> options,
        ILogger<LogProcessingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LogProcessingBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var opts = _options.CurrentValue;

            try
            {
                var processed = await ProcessBatchAsync(opts.BatchSize, stoppingToken);
                if (processed > 0)
                    _logger.LogInformation("Processed {Count} log(s) in this cycle.", processed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing log batch.");
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

        _logger.LogInformation("LogProcessingBackgroundService stopping.");
    }

    private async Task<int> ProcessBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var logs = scope.ServiceProvider.GetRequiredService<ILogRepository>();
        var grouping = scope.ServiceProvider.GetRequiredService<IIssueGroupingService>();
        var analyzer = scope.ServiceProvider.GetRequiredService<ILogAnalyzer>();

        var batch = await logs.GetUnprocessedBatchAsync(batchSize, cancellationToken);
        if (batch.Count == 0) return 0;

        foreach (var log in batch)
        {
            var analysis = analyzer.Analyze(new LogAnalysisInput(log.Message, log.StackTrace));

            log.Pattern = analysis.Pattern;
            log.Score = analysis.Score;

            var issue = await grouping.GroupAsync(log, analysis, cancellationToken);
            if (issue is not null) log.Issue = issue;

            log.Processed = true;
            log.ProcessedAt = DateTime.UtcNow;
        }

        await logs.SaveChangesAsync(cancellationToken);
        return batch.Count;
    }
}
