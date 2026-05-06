using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Infrastructure.Persistence;
using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogMin.Infrastructure.Repositories;

public sealed class AgentSettingRepository : IAgentSettingRepository
{
    private const int SingletonId = 1;

    private readonly AppDbContext _db;

    public AgentSettingRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<AgentSetting?> GetAsync(CancellationToken cancellationToken = default)
    {
        return _db.AgentSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == SingletonId, cancellationToken);
    }

    public async Task UpsertAsync(AgentSetting setting, CancellationToken cancellationToken = default)
    {
        var existing = await _db.AgentSettings.FirstOrDefaultAsync(x => x.Id == SingletonId, cancellationToken);
        if (existing is null)
        {
            setting.Id = SingletonId;
            setting.UpdatedAt = DateTime.UtcNow;
            await _db.AgentSettings.AddAsync(setting, cancellationToken);
            return;
        }

        existing.Model = setting.Model;
        existing.ApiKey = setting.ApiKey;
        existing.ApiServerUrl = setting.ApiServerUrl;
        existing.UpdatedAt = DateTime.UtcNow;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
