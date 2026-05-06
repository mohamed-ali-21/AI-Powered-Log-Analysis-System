using LogMin.Application.DTOs;

namespace LogMin.Application.Services.Abstruction;

public interface ILogIngestionService
{
    Task<IngestLogResponse> IngestAsync(IngestLogRequest request, CancellationToken cancellationToken = default);
}
