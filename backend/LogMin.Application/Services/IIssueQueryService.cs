using LogMin.Application.DTOs.Common;
using LogMin.Application.DTOs.Issues;

namespace LogMin.Application.Services;

public interface IIssueQueryService
{
    Task<PagedResult<IssueDto>> SearchAsync(IssuesQuery query, CancellationToken cancellationToken = default);
    Task<IssueDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
