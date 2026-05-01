using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogMin.Infrastructure.Persistence.Configurations;

public sealed class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("Issues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Pattern).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ServiceName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.FirstSeen).IsRequired();
        builder.Property(x => x.LastSeen).IsRequired();
        builder.Property(x => x.Count).IsRequired();
        builder.Property(x => x.AvgScore).IsRequired();
        builder.Property(x => x.RepresentativeLogId).IsRequired();
        builder.Property(x => x.IsAiProcessed).IsRequired();

        builder.HasIndex(x => new { x.Pattern, x.ServiceName, x.LastSeen });
        builder.HasIndex(x => x.LastSeen);
        builder.HasIndex(x => x.IsAiProcessed);
    }
}
