using LogMin.Infrastructure.Abstractions.Intelligence;
using LogMin.Infrastructure.Persistence.Entities;

namespace LogMin.Application.Services;

public interface IIssueGroupingService
{
    Task<Issue?> GroupAsync(Log log, LogAnalysisOutput analysis, CancellationToken cancellationToken = default);
}
