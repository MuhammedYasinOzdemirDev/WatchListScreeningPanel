namespace WatchListScreening.Application.DTOs.Harvest;

public class RawScrapedItem
{
    public string RawFullName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? RawFirstName { get; set; }
    public string? RawLastName { get; set; }
    public string? CleanedFullName { get; set; }
    public string? CleanedFirstName { get; set; }
    public string? CleanedLastName { get; set; }
    public string? ContentHash { get; set; }
    public string? EntityTypeStr { get; set; }
    public string? AdditionalData { get; set; }
}
