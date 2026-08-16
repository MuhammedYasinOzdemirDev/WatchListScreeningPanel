namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Message published to RabbitMQ to trigger a harvest job.
/// Consumed by WatchListScreening.Scraper worker service.
/// </summary>
public class HarvestCommandMessage
{
    public int ListSourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ScraperType { get; set; } = string.Empty;
    public string? ScraperConfig { get; set; }
    public string TriggeredBy { get; set; } = "Scheduled";
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}
