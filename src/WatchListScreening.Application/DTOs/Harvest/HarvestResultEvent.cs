namespace WatchListScreening.Application.DTOs.Harvest;

public class HarvestResultEvent
{
    public int ListSourceId { get; set; }
    public int ListSourceRunId { get; set; }
    public bool IsSuccess { get; set; }
    public int TotalScraped { get; set; }
    public int TotalInserted { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompletedAt { get; set; }
}
