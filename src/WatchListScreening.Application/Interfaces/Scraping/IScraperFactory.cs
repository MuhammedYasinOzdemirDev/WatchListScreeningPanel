using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Application.Interfaces.Scraping;

public interface IScraperFactory
{
    /// <summary>
    /// Resolves the correct scraper implementation based on the command.
    /// Uses Reflection on ScraperClassName to find the correct instance.
    /// </summary>
    ISourceScraper Create(HarvestCommandDto command);
}
