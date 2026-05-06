using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Application.DTOs;
using LogMin.Infrastructure.Persistence.Entities;
using LogMin.Application.Services.Abstruction;

namespace LogMin.Application.Services;

public sealed class LogIngestionService : ILogIngestionService
{
    private readonly ILogRepository _logs;

    public LogIngestionService(ILogRepository logs)
    {
        _logs = logs;
    }

    public async Task<IngestLogResponse> IngestAsync(IngestLogRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Log
        {
            Id = Guid.NewGuid(),
            Message = request.Message,
            StackTrace = request.StackTrace,
            ServiceName = request.ServiceName,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            Processed = false
        };

        await _logs.AddAsync(entity, cancellationToken);
        await _logs.SaveChangesAsync(cancellationToken);

        return new IngestLogResponse
        {
            Id = entity.Id,
            Timestamp = entity.Timestamp
        };
    }
}
