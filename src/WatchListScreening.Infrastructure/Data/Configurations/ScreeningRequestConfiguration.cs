using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the ScreeningRequest entity.
/// Stores every screening request initiated by users.
/// </summary>
public class ScreeningRequestConfiguration : IEntityTypeConfiguration<ScreeningRequest>
{
    public void Configure(EntityTypeBuilder<ScreeningRequest> builder)
    {
        // --------------------------------------------------------------------
        // TABLE CONFIGURATION
        // --------------------------------------------------------------------

        // Maps this entity to the "ScreeningRequests" table.
        builder.ToTable("ScreeningRequests");

        // Configures the primary key.
        builder.HasKey(x => x.Id);

        // --------------------------------------------------------------------
        // PROPERTY CONFIGURATION
        // --------------------------------------------------------------------

        // Name or organization entered by the user.
        // This is the value that will be screened against sanction lists.
        builder.Property(x => x.SearchQuery)
            .IsRequired()
            .HasMaxLength(500);

        // Indicates whether the request is for an
        // individual or an organization.
        builder.Property(x => x.SearchType)
            .IsRequired();

        // User who initiated the screening request.
        builder.Property(x => x.RequestedBy)
            .IsRequired()
            .HasMaxLength(200);

        // RequestedAt kaldırıldı — BaseEntity.CreatedAt kullanılıyor

        // Completion time of the screening process.
        builder.Property(x => x.CompletedAt);

        // Current processing status.
        // Examples: Pending, Processing, Completed, Failed.
        builder.Property(x => x.Status)
            .IsRequired();

        // Total number of matches found.
        builder.Property(x => x.TotalMatches);

        // Indicates whether this is a bulk screening request.
        builder.Property(x => x.IsBulk)
            .IsRequired();

        // Optional note entered by the user.
        builder.Property(x => x.Notes);

        // Record creation timestamp.
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Last update timestamp.
        builder.Property(x => x.UpdatedAt);

        // --------------------------------------------------------------------
        // RELATIONSHIP CONFIGURATION
        // --------------------------------------------------------------------

        // One screening request can produce multiple screening results.
        builder.HasMany(x => x.Results)
            .WithOne(x => x.ScreeningRequest)
            .HasForeignKey(x => x.ScreeningRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // --------------------------------------------------------------------
        // INDEX CONFIGURATION
        // --------------------------------------------------------------------

        // Used to retrieve pending or processing requests efficiently.
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_ScreeningRequests_Status");

        // Date-based filtering uses CreatedAt (BaseEntity convention)

        // Used to display a user's screening history.
        builder.HasIndex(x => x.RequestedBy)
            .HasDatabaseName("IX_ScreeningRequests_RequestedBy");
    }
}