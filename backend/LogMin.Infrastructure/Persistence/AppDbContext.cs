using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogMin.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Log> Logs => Set<Log>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssueAnalysis> IssueAnalyses => Set<IssueAnalysis>();
    public DbSet<AgentSetting> AgentSettings => Set<AgentSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
