namespace LogMin.Infrastructure.Persistence.Entities;

public sealed class AgentSetting
{
    public int Id { get; set; }
    public string Model { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiServerUrl { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}
