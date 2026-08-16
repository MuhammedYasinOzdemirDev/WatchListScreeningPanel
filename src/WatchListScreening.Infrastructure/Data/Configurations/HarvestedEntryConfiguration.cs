using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Configurations;

public class HarvestedEntryConfiguration : IEntityTypeConfiguration<HarvestedEntry>
{
    public void Configure(EntityTypeBuilder<HarvestedEntry> builder)
    {
        builder.ToTable("HarvestedEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RawFullName).IsRequired().HasMaxLength(500);
        builder.Property(x => x.RawFirstName).HasMaxLength(250);
        builder.Property(x => x.RawLastName).HasMaxLength(250);
        builder.Property(x => x.RawCountry).HasMaxLength(100);
        builder.Property(x => x.CleanedFullName).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CleanedFirstName).HasMaxLength(250);
        builder.Property(x => x.CleanedLastName).HasMaxLength(250);
        builder.Property(x => x.DateOfBirth).HasMaxLength(50);
        builder.Property(x => x.NationalId).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.Aliases);       // JSON — no length limit
        builder.Property(x => x.Passports);     // JSON
        builder.Property(x => x.Addresses);     // JSON
        builder.Property(x => x.Positions);     // JSON
        builder.Property(x => x.AdditionalData); // JSON
        builder.Property(x => x.ContentHash).IsRequired().HasMaxLength(64);
        builder.Property(x => x.IsProcessed).HasDefaultValue(false);
        builder.Property(x => x.ProcessedAt);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        // FK: HarvestedEntry → ListSource (Cascade)
        builder.HasOne(x => x.ListSource)
            .WithMany(x => x.HarvestedEntries)
            .HasForeignKey(x => x.ListSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK: HarvestedEntry → ListSourceRun (Cascade)
        builder.HasOne(x => x.ListSourceRun)
            .WithMany(x => x.HarvestedEntries)
            .HasForeignKey(x => x.ListSourceRunId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK: HarvestedEntry → SanctionEntry (SetNull — sanction silinince entry kalır)
        builder.HasOne(x => x.SanctionEntry)
            .WithMany(x => x.HarvestedEntries)
            .HasForeignKey(x => x.SanctionEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        // UNIQUE: Aynı hash tekrar insert edilemesin
        builder.HasIndex(x => x.ContentHash)
            .IsUnique()
            .HasDatabaseName("IX_HarvestedEntries_ContentHash_UNIQUE");

        builder.HasIndex(x => x.ListSourceId).HasDatabaseName("IX_HarvestedEntries_ListSourceId");
        builder.HasIndex(x => x.IsProcessed).HasDatabaseName("IX_HarvestedEntries_IsProcessed");
        builder.HasIndex(x => x.CleanedFullName).HasDatabaseName("IX_HarvestedEntries_CleanedFullName");
    }
}
