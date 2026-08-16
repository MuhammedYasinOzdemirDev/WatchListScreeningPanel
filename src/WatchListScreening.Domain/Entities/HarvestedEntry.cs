using WatchListScreening.Domain.Common;
using WatchListScreening.Domain.Enums;
namespace WatchListScreening.Domain.Entities;

/// <summary>
/// Staging area for raw data fetched by scrapers.
/// Data here is NOT yet in SanctionEntries — it awaits processing/approval.
/// Deduplication is enforced via ContentHash (SHA256, UNIQUE index).
/// </summary>
public class HarvestedEntry : BaseEntity
{
    public int ListSourceId { get; set; }
    public int ListSourceRunId { get; set; }

    /// <summary>FK to SanctionEntry — set after the record is processed and promoted.</summary>
    public int? SanctionEntryId { get; set; }

    // --- Raw (as scraped from source) ---
    public string RawFullName { get; set; } = null!;
    public string? RawFirstName { get; set; }
    public string? RawLastName { get; set; }
    public string? RawCountry { get; set; }

    // --- Cleaned (after normalization pipeline) ---
    public string CleanedFullName { get; set; } = null!;
    public string? CleanedFirstName { get; set; }
    public string? CleanedLastName { get; set; }
    public EntityType? EntityType { get; set; }
    public SourceCategory? Category { get; set; }

    // --- Structured fields (nullable, from source if available) ---
    /// <summary>Stored as string intentionally (format varies per source). SanctionEntry.DateOfBirth is DateOnly.</summary>
    public string? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? Country { get; set; }

    /// <summary>JSON array of alias strings. e.g. ["Ali H.", "A. Hassan"]</summary>
    public string? Aliases { get; set; }

    /// <summary>JSON array of passport objects.</summary>
    public string? Passports { get; set; }

    /// <summary>JSON array of address objects.</summary>
    public string? Addresses { get; set; }

    /// <summary>JSON array of political/professional positions (for PEPs).</summary>
    public string? Positions { get; set; }

    /// <summary>Any extra source-specific data in JSON.</summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// SHA256 hash of (CleanedFullName + ListSourceId + DateOfBirth + NationalId).
    /// UNIQUE index — prevents duplicate inserts from repeated scrapes.
    /// </summary>
    public string ContentHash { get; set; } = null!;

    /// <summary>True once this entry has been promoted to SanctionEntries.</summary>
    public bool IsProcessed { get; set; } = false;

    /// <summary>Timestamp when processing/promotion occurred.</summary>
    public DateTime? ProcessedAt { get; set; }

    // Navigation
    public ListSource ListSource { get; set; } = null!;
    public ListSourceRun ListSourceRun { get; set; } = null!;
    public SanctionEntry? SanctionEntry { get; set; }
}
