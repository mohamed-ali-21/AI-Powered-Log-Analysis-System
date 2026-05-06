using LogMin.Infrastructure.Persistence.Entities;

namespace LogMin.Infrastructure.Abstractions.Persistence;

public interface IAgentSettingRepository
{
    Task<AgentSetting?> GetAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(AgentSetting setting, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
