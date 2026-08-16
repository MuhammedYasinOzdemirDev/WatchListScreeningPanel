using WatchListScreening.Domain.Common;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Domain.Entities;

/// <summary>
/// Represents a registered data source to harvest sanction/PEP data from.
/// Each source has its own scraper configuration and schedule.
/// </summary>
public class ListSource : BaseEntity
{
    /// <summary>Display name. e.g. "OFAC SDN List"</summary>
    public string Name { get; set; } = null!;

    /// <summary>Target URL to scrape or download from.</summary>
    public string Url { get; set; } = null!;

    /// <summary>Type of data in this source.</summary>
    public SourceCategory Category { get; set; }

    /// <summary>Which scraping strategy to use.</summary>
    public ScraperType ScraperType { get; set; }

    /// <summary>
    /// JSON config for the scraper (CSS selectors, XPath, column names, etc.)
    /// Parsed by the scraper implementation at runtime.
    /// </summary>
    public string? ScraperConfig { get; set; }

    /// <summary>Cron expression for scheduled harvesting. e.g. "0 */6 * * *"</summary>
    public string? CronExpression { get; set; }

    /// <summary>Hangfire job ID. Format: "harvest-source-{Id}"</summary>
    public string? HangfireJobId { get; set; }

    /// <summary>HTTP/Selenium request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>Number of retry attempts on failure.</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Whether a concrete scraper implementation exists for this source.
    /// If false, Hangfire will NOT schedule this source even if active.
    /// </summary>
    public bool HasScraperImpl { get; set; } = false;

    /// <summary>Whether this source is enabled for harvesting.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Timestamp of the last successful harvest.</summary>
    public DateTime? LastHarvestAt { get; set; }

    /// <summary>Optional description or notes about the source.</summary>
    public string? Notes { get; set; }

    // Navigation
    public ICollection<ListSourceRun> Runs { get; set; } = new List<ListSourceRun>();
    public ICollection<HarvestedEntry> HarvestedEntries { get; set; } = new List<HarvestedEntry>();
    public ICollection<SanctionEntry> SanctionEntries { get; set; } = new List<SanctionEntry>();
}