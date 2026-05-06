using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogMin.Infrastructure.Persistence.Configurations;

public sealed class AgentSettingConfiguration : IEntityTypeConfiguration<AgentSetting>
{
    public void Configure(EntityTypeBuilder<AgentSetting> builder)
    {
        builder.ToTable("AgentSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Model).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ApiKey).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ApiServerUrl).IsRequired().HasMaxLength(500);
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
