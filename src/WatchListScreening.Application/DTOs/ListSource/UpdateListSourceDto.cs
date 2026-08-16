using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;
public class UpdateListSourceDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public SourceCategory Category { get; set; }
    public ScraperType ScraperType { get; set; }
    public string? CronExpression { get; set; }
    public int TimeoutSeconds { get; set; }
    public int RetryCount { get; set; }
    public bool IsActive { get; set; }
    public bool HasScraperImpl { get; set; }
    public string? Notes { get; set; }
}
