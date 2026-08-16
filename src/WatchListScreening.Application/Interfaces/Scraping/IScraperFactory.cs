using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.Interfaces.Scraping;

/// <summary>
/// Factory that resolves the correct ISourceScraper based on ScraperType enum.
/// Implementation lives in Infrastructure/Scraper project.
/// </summary>
public interface IScraperFactory
{
    ISourceScraper Create(ScraperType scraperType);
}
