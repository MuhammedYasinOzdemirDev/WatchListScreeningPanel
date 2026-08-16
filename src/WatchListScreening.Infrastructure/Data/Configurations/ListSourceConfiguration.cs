using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Configurations;

public class ListSourceConfiguration : IEntityTypeConfiguration<ListSource>
{
    public void Configure(EntityTypeBuilder<ListSource> builder)
    {
        builder.ToTable("ListSources");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Category).IsRequired();
        builder.Property(x => x.ScraperType).IsRequired();
        builder.Property(x => x.ScraperConfig);
        builder.Property(x => x.ScraperClassName).HasMaxLength(200);
        builder.Property(x => x.CronExpression).HasMaxLength(100);
        builder.Property(x => x.HangfireJobId).HasMaxLength(200);
        builder.Property(x => x.TimeoutSeconds).HasDefaultValue(120);
        builder.Property(x => x.RetryCount).HasDefaultValue(3);
        builder.Property(x => x.HasScraperImpl).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.Name).HasDatabaseName("IX_ListSources_Name");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_ListSources_IsActive");
        builder.HasIndex(x => x.Category).HasDatabaseName("IX_ListSources_Category");
    }
}
