namespace WatchListScreening.Application.DTOs.Harvest;

/// <summary>
/// Defines a complex scraping scenario beyond a simple URL fetch.
/// Parses the JSON stored in ListSource.ScraperConfig.
/// </summary>
public class ScrapingConfig
{
    /// <summary>
    /// Sequential steps to reach the target data (e.g. login, click search, wait).
    /// </summary>
    public List<ScrapingStep> Steps { get; set; } = new();

    /// <summary>
    /// The CSS or XPath selector to find individual record rows (after Steps are completed).
    /// </summary>
    public string RowSelector { get; set; } = string.Empty;

    /// <summary>
    /// Map of target field (e.g. "FullName") to its selector relative to the RowSelector (e.g. ".name-col").
    /// </summary>
    public Dictionary<string, string> FieldSelectors { get; set; } = new();
}

public class ScrapingStep
{
    /// <summary>
    /// Action type: "navigate", "wait", "click", "input", "submit"
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Target selector (CSS/XPath) for "click", "input", "submit". 
    /// Target URL for "navigate".
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// The text to type for "input" action.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Duration in milliseconds for "wait" action.
    /// </summary>
    public int? DurationMs { get; set; }
}
