using WatchListScreening.Domain.Common;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Domain.Entities;

/// <summary>
/// Records a single execution of a harvest job for a given ListSource.
/// Tracks timing, outcome, and counts for auditing and monitoring.
/// </summary>
public class ListSourceRun : BaseEntity
{
    public int ListSourceId { get; set; }

    public HarvestStatus Status { get; set; } = HarvestStatus.Running;

    /// <summary>How the run was triggered: "Scheduled" or "Manual"</summary>
    public string TriggeredBy { get; set; } = "Scheduled";

    /// <summary>When the run started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>When the run finished (null if still running).</summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>Total duration in milliseconds.</summary>
    public long? DurationMs { get; set; }

    /// <summary>Total records fetched from the source.</summary>
    public int TotalScraped { get; set; }

    /// <summary>Records inserted as new SanctionEntries.</summary>
    public int TotalNew { get; set; }

    /// <summary>Records that updated existing SanctionEntries.</summary>
    public int TotalUpdated { get; set; }

    /// <summary>Records skipped because the hash matched (no change).</summary>
    public int TotalSkipped { get; set; }

    /// <summary>Error message if status is Failed or PartialSuccess.</summary>
    public string? ErrorMessage { get; set; }

    // Navigation
    public ListSource ListSource { get; set; } = null!;
    public ICollection<HarvestedEntry> HarvestedEntries { get; set; } = new List<HarvestedEntry>();
}
