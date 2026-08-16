namespace WatchListScreening.Application.DTOs;

public class HarvestedEntryDto
{
    public int Id { get; set; }
    public int ListSourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string RawFullName { get; set; } = string.Empty;
    public string CleanedFullName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? DateOfBirth { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Shown in list view — truncated hash for display
    public string HashPrefix => ContentHash.Length >= 8 ? ContentHash[..8] : ContentHash;
    public string ContentHash { get; set; } = string.Empty;
}
