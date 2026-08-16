namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Normalized and cleaned version of RawScrapedItem.
/// Ready to be compared via ContentHash and stored in HarvestedEntries.
/// </summary>
public class CleanedItem
{
    public string CleanedFullName { get; set; } = string.Empty;
    public string? CleanedFirstName { get; set; }
    public string? CleanedLastName { get; set; }
    public string? Country { get; set; }
    public string? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? AliasesJson { get; set; }
    public string? AdditionalDataJson { get; set; }

    /// <summary>
    /// SHA256 of (CleanedFullName + ListSourceId + DateOfBirth + NationalId).
    /// Used for deduplication in HarvestedEntries.
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;
}
