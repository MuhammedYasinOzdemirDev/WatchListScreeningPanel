using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the AuditLog entity.
/// Stores all critical system actions for audit and regulatory compliance.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // --------------------------------------------------------------------
        // TABLE CONFIGURATION
        // --------------------------------------------------------------------

        // Maps this entity to the "AuditLogs" table.
        builder.ToTable("AuditLogs");

        // Configures the primary key.
        builder.HasKey(x => x.Id);

        // --------------------------------------------------------------------
        // PROPERTY CONFIGURATION
        // --------------------------------------------------------------------

        // Action performed by the user.
        // Examples: Create, Update, Delete, Screen, Review, Export.
        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(100);

        // Entity on which the action was performed.
        // Examples: SanctionEntry, ScreeningRequest, ScreeningResult.
        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        // Related entity identifier.
        builder.Property(x => x.EntityId);

        // User who performed the action.
        builder.Property(x => x.PerformedBy)
            .IsRequired()
            .HasMaxLength(200);

        // Date and time when the action occurred.
        builder.Property(x => x.PerformedAt)
            .IsRequired();

        // Previous values before the update.
        // Stored as JSON for audit purposes.
        builder.Property(x => x.OldValues);

        // New values after the update.
        // Stored as JSON for audit purposes.
        builder.Property(x => x.NewValues);

        // Client IP address.
        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        // Optional additional information.
        builder.Property(x => x.Details);

        // --------------------------------------------------------------------
        // INDEX CONFIGURATION
        // --------------------------------------------------------------------

        // Used for date-based reporting and audit queries.
        builder.HasIndex(x => x.PerformedAt)
            .HasDatabaseName("IX_AuditLogs_PerformedAt");

        // Used to filter logs by action type.
        builder.HasIndex(x => x.Action)
            .HasDatabaseName("IX_AuditLogs_Action");

        // Composite index used to retrieve the history
        // of a specific entity efficiently.
        builder.HasIndex(x => new { x.EntityType, x.EntityId })
            .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");
    }
}