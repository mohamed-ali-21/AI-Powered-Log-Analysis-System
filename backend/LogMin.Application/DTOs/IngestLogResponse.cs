namespace LogMin.Application.DTOs;

public sealed class IngestLogResponse
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }
}
