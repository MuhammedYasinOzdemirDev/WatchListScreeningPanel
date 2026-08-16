using WatchListScreening.Application.DTOs;

namespace WatchListScreening.Application.Interfaces.Scraping;

/// <summary>
/// Contract for all scraper implementations (Http, Selenium, Api, File).
/// Implementations live in WatchListScreening.Scraper project — NOT here.
/// This interface stays in Application so Application can publish commands
/// without knowing the concrete scraper.
/// </summary>
public interface ISourceScraper
{
    Task<List<RawScrapedItem>> ScrapeAsync(
        string url,
        string? scraperConfig,
        CancellationToken cancellationToken = default);
}
