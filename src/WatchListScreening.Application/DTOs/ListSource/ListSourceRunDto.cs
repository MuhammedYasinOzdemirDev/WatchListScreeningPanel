using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;

public class ListSourceRunDto
{
    public int Id { get; set; }
    public int ListSourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public HarvestStatus Status { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public long? DurationMs { get; set; }
    public int TotalScraped { get; set; }
    public int TotalNew { get; set; }
    public int TotalUpdated { get; set; }
    public int TotalSkipped { get; set; }
    public string? ErrorMessage { get; set; }
}
