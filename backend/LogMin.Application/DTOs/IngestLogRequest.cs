using System.ComponentModel.DataAnnotations;

namespace LogMin.Application.DTOs;

public sealed class IngestLogRequest
{
    [Required, StringLength(8000, MinimumLength = 1)]
    public string Message { get; set; } = default!;

    public string? StackTrace { get; set; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string ServiceName { get; set; } = default!;

    public DateTime? Timestamp { get; set; }
}
