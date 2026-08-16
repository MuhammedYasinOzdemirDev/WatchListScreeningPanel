using WatchListScreening.Application.DTOs;

namespace WatchListScreening.Application.Interfaces.Scraping;

/// <summary>
/// Orchestrates the cleaning pipeline for raw scraped items.
/// Pipeline: TrimWhitespace › NormalizeUnicode › RemoveSpecialChars › TitleCase
/// </summary>
public interface IDataCleaner
{
    CleanedItem Clean(RawScrapedItem raw);
}
