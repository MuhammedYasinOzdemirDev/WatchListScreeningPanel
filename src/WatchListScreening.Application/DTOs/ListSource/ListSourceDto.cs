using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;

public class ListSourceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public SourceCategory Category { get; set; }
    public ScraperType ScraperType { get; set; }
    public string? CronExpression { get; set; }
    public int TimeoutSeconds { get; set; }
    public int RetryCount { get; set; }
    public bool HasScraperImpl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastHarvestAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Computed from last run
    public string? LastRunStatus { get; set; }
    public int TotalHarvestedEntries { get; set; }
}
