using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Configurations;

public class ListSourceRunConfiguration : IEntityTypeConfiguration<ListSourceRun>
{
    public void Configure(EntityTypeBuilder<ListSourceRun> builder)
    {
        builder.ToTable("ListSourceRuns");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.TriggeredBy).IsRequired().HasMaxLength(50);
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.FinishedAt);
        builder.Property(x => x.DurationMs);
        builder.Property(x => x.TotalScraped).HasDefaultValue(0);
        builder.Property(x => x.TotalNew).HasDefaultValue(0);
        builder.Property(x => x.TotalUpdated).HasDefaultValue(0);
        builder.Property(x => x.TotalSkipped).HasDefaultValue(0);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        // FK: ListSourceRun → ListSource (Cascade: source silinince run'ları da silinsin)
        builder.HasOne(x => x.ListSource)
            .WithMany(x => x.Runs)
            .HasForeignKey(x => x.ListSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ListSourceId).HasDatabaseName("IX_ListSourceRuns_ListSourceId");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_ListSourceRuns_Status");
    }
}
