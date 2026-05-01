using LogMin.Application.DTOs;

namespace LogMin.Application.Services;

public interface ILogIngestionService
{
    Task<IngestLogResponse> IngestAsync(IngestLogRequest request, CancellationToken cancellationToken = default);
}
