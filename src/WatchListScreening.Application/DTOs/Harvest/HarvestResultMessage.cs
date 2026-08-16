namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Message published back to RabbitMQ after harvest completes.
/// Consumed by API to update ListSourceRun status.
/// </summary>
public class HarvestResultMessage
{
    public int ListSourceId { get; set; }
    public int ListSourceRunId { get; set; }
    public bool Success { get; set; }
    public int TotalScraped { get; set; }
    public int TotalNew { get; set; }
    public int TotalUpdated { get; set; }
    public int TotalSkipped { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public long DurationMs { get; set; }
}
