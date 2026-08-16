using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs.Harvest;

public class HarvestCommandDto
{
    public int ListSourceId { get; set; }
    public int ListSourceRunId { get; set; }
    public string ScraperClassName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ScraperConfigJson { get; set; }
    public ScraperType ScraperType { get; set; }
    public int TimeoutSeconds { get; set; }
    public int RetryCount { get; set; }
}
