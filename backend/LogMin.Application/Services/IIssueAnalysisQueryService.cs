using LogMin.Application.DTOs.AiAnalysis;
using LogMin.Application.DTOs.Common;

namespace LogMin.Application.Services;

public interface IIssueAnalysisQueryService
{
    Task<PagedResult<IssueAnalysisDto>> SearchAsync(AiAnalysisQuery query, CancellationToken cancellationToken = default);
    Task<IssueAnalysisDto?> GetByIssueIdAsync(Guid issueId, CancellationToken cancellationToken = default);
    Task<bool> ScheduleRetryAsync(Guid issueId, CancellationToken cancellationToken = default);
}
