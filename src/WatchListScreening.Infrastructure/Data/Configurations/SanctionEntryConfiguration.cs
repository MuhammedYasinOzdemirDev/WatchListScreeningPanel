using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the SanctionEntry entity.
/// Responsible for table mapping, property constraints and indexes.
/// </summary>
public class SanctionEntryConfiguration : IEntityTypeConfiguration<SanctionEntry>
{
    public void Configure(EntityTypeBuilder<SanctionEntry> builder)
    {
        // --------------------------------------------------------------------
        // TABLE CONFIGURATION
        // --------------------------------------------------------------------

        // Maps this entity to the "SanctionEntries" table.
        builder.ToTable("SanctionEntries");

        // Configures the primary key.
        builder.HasKey(x => x.Id);

        // --------------------------------------------------------------------
        // PROPERTY CONFIGURATION
        // --------------------------------------------------------------------

        // Full name is the primary field used during screening.
        // Almost every screening query searches this column.
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(500);

        // Optional first name.
        // Used for partial matching algorithms.
        builder.Property(x => x.FirstName)
            .HasMaxLength(250);

        // Optional last name.
        // Used together with FirstName during screening.
        builder.Property(x => x.LastName)
            .HasMaxLength(250);

        // Indicates whether the record belongs to a person or organization.
        builder.Property(x => x.EntityType)
            .IsRequired();

        // Country information used for regional filtering.
        builder.Property(x => x.Country)
            .HasMaxLength(100);

        // Optional date of birth.
        // Helps distinguish people with identical names.
        builder.Property(x => x.DateOfBirth);

        // National identification number.
        // Used when exact identity verification is required.
        builder.Property(x => x.NationalId)
            .HasMaxLength(50);

        // Source sanction list.
        // Examples: OFAC, UN, EU, MASAK...
        builder.Property(x => x.ListSource)
            .IsRequired()
            .HasMaxLength(200);

        // Official source URL.
        builder.Property(x => x.ListSourceUrl)
            .HasMaxLength(1000);

        // JSON array containing aliases.
        builder.Property(x => x.Aliases);

        // JSON field for additional information such as
        // passport numbers, addresses and related persons.
        builder.Property(x => x.AdditionalInfo);

        // Indicates whether the sanction record is currently active.
        builder.Property(x => x.IsActive)
            .IsRequired();

        // Date when the sanction became active.
        builder.Property(x => x.AddedAt)
            .IsRequired();

        // Date when the sanction was removed.
        builder.Property(x => x.DeactivatedAt);

        // Record creation timestamp.
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Last update timestamp.
        builder.Property(x => x.UpdatedAt);

        // --------------------------------------------------------------------
        // INDEX CONFIGURATION
        // --------------------------------------------------------------------

        // Most important index.
        // Every screening operation searches FullName.
        builder.HasIndex(x => x.FullName)
            .HasDatabaseName("IX_SanctionEntries_FullName");

        // Used when filtering records by sanction source.
        builder.HasIndex(x => x.ListSource)
            .HasDatabaseName("IX_SanctionEntries_ListSource");

        // Frequently used to retrieve only active sanctions.
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_SanctionEntries_IsActive");

        // Used for country-based screening.
        builder.HasIndex(x => x.Country)
            .HasDatabaseName("IX_SanctionEntries_Country");
    }
}