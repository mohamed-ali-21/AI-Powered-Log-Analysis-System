using LogMin.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogMin.Infrastructure.Persistence.Configurations;

public sealed class IssueAnalysisConfiguration : IEntityTypeConfiguration<IssueAnalysis>
{
    public void Configure(EntityTypeBuilder<IssueAnalysis> builder)
    {
        builder.ToTable("IssueAnalyses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.RootCause);
        builder.Property(x => x.Impact);
        builder.Property(x => x.Recommendation);
        builder.Property(x => x.Severity).HasMaxLength(20);
        builder.Property(x => x.Summary);
        builder.Property(x => x.Tags);
        builder.Property(x => x.AIConfidence);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Issue)
            .WithMany(x => x.Analyses)
            .HasForeignKey(x => x.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.IssueId);
    }
}
