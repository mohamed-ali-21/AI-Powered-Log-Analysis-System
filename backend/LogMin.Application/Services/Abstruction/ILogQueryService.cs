using LogMin.Application.DTOs.Common;
using LogMin.Application.DTOs.Logs;

namespace LogMin.Application.Services.Abstruction;

public interface ILogQueryService
{
    Task<PagedResult<LogDto>> SearchAsync(LogsQuery query, CancellationToken cancellationToken = default);
    Task<LogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
