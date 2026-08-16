using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Application.Interfaces.Scraping;

public interface ISourceScraper
{
    Task<List<RawScrapedItem>> ScrapeAsync(HarvestCommandDto command, CancellationToken cancellationToken = default);
}
