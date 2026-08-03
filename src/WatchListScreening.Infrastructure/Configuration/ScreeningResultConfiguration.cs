using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the ScreeningResult entity.
/// Stores every match produced by a screening request.
/// </summary>
public class ScreeningResultConfiguration : IEntityTypeConfiguration<ScreeningResult>
{
    public void Configure(EntityTypeBuilder<ScreeningResult> builder)
    {
        // --------------------------------------------------------------------
        // TABLE CONFIGURATION
        // --------------------------------------------------------------------

        // Maps this entity to the "ScreeningResults" table.
        builder.ToTable("ScreeningResults");

        // Configures the primary key.
        builder.HasKey(x => x.Id);

        // --------------------------------------------------------------------
        // PROPERTY CONFIGURATION
        // --------------------------------------------------------------------

        // Match score between 0.00 and 100.00.
        // Decimal precision allows more accurate ranking of matches.
        builder.Property(x => x.MatchScore)
            .HasPrecision(5, 2)
            .IsRequired();

        // Matching algorithm used to generate this result.
        // Examples: Exact, Fuzzy, Contains, Phonetic.
        builder.Property(x => x.MatchedType)
            .IsRequired();

        // Automatically calculated risk level.
        builder.Property(x => x.RiskLevel)
            .IsRequired();

        // Current compliance review status.
        builder.Property(x => x.ReviewStatus)
            .IsRequired();

        // Compliance officer who reviewed the result.
        builder.Property(x => x.ReviewedBy)
            .HasMaxLength(200);

        // Date and time of the review.
        builder.Property(x => x.ReviewedAt);

        // Reviewer notes such as
        // "False Positive" or "Confirmed Match".
        builder.Property(x => x.ReviewNotes);

        // Record creation timestamp.
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Last update timestamp.
        builder.Property(x => x.UpdatedAt);

        // --------------------------------------------------------------------
        // RELATIONSHIP CONFIGURATION
        // --------------------------------------------------------------------

        // One ScreeningRequest can produce many ScreeningResults.
        builder.HasOne(x => x.ScreeningRequest)
            .WithMany(x => x.Results)
            .HasForeignKey(x => x.ScreeningRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // One SanctionEntry can match many ScreeningResults.
        builder.HasOne(x => x.SanctionEntry)
            .WithMany(x => x.ScreeningResults)
            .HasForeignKey(x => x.SanctionEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        // --------------------------------------------------------------------
        // INDEX CONFIGURATION
        // --------------------------------------------------------------------

        // Used to retrieve all results belonging to a screening request.
        builder.HasIndex(x => x.ScreeningRequestId)
            .HasDatabaseName("IX_ScreeningResults_ScreeningRequestId");

        // Used to list pending compliance reviews.
        builder.HasIndex(x => x.ReviewStatus)
            .HasDatabaseName("IX_ScreeningResults_ReviewStatus");

        // Used for filtering results by calculated risk level.
        builder.HasIndex(x => x.RiskLevel)
            .HasDatabaseName("IX_ScreeningResults_RiskLevel");

        // Used to determine how many times
        // a sanction record has matched.
        builder.HasIndex(x => x.SanctionEntryId)
            .HasDatabaseName("IX_ScreeningResults_SanctionEntryId");
    }
}