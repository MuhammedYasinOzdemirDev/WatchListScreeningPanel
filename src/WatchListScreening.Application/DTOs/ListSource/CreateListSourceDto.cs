using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;
public class CreateListSourceDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public SourceCategory Category { get; set; }
    public ScraperType ScraperType { get; set; }
    public string? CronExpression { get; set; }
    public int TimeoutSeconds { get; set; } = 120;
    public int RetryCount { get; set; } = 3;
    public string? Notes { get; set; }
}
