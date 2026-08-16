namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Raw data as scraped from the source — no cleaning applied yet.
/// All fields string because source data is unpredictable.
/// </summary>
public class RawScrapedItem
{
    public string RawFullName { get; set; } = string.Empty;
    public string? RawFirstName { get; set; }
    public string? RawLastName { get; set; }
    public string? RawCountry { get; set; }
    public string? RawDateOfBirth { get; set; }
    public string? RawNationalId { get; set; }

    /// <summary>JSON array string — aliases as scraped.</summary>
    public string? RawAliases { get; set; }

    /// <summary>Any extra source-specific fields in key-value pairs.</summary>
    public Dictionary<string, string> ExtraFields { get; set; } = new();
}
