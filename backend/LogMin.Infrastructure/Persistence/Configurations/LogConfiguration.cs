using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogMin.Infrastructure.Persistence.Configurations;

public sealed class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("Logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.StackTrace);
        builder.Property(x => x.ServiceName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.Processed).IsRequired();
        builder.Property(x => x.ProcessedAt);
        builder.Property(x => x.Pattern).HasMaxLength(100);
        builder.Property(x => x.Score);

        builder.HasOne(x => x.Issue)
            .WithMany(x => x.Logs)
            .HasForeignKey(x => x.IssueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Processed);
        builder.HasIndex(x => new { x.Processed, x.Timestamp });
        builder.HasIndex(x => new { x.ServiceName, x.Timestamp });
        builder.HasIndex(x => x.IssueId);
    }
}
