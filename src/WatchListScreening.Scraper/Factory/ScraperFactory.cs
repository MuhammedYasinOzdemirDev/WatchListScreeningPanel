using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Application.Interfaces.Scraping;

namespace WatchListScreening.Scraper.Factory;

public class ScraperFactory(IEnumerable<ISourceScraper> scrapers, ILogger<ScraperFactory> logger) : IScraperFactory
{
    private readonly IEnumerable<ISourceScraper> _scrapers = scrapers;
    private readonly ILogger<ScraperFactory> _logger = logger;

    public ISourceScraper Create(HarvestCommandDto command)
    {
        if (string.IsNullOrWhiteSpace(command.ScraperClassName))
        {
            _logger.LogError("ScraperClassName is empty for ListSourceId {ListSourceId}", command.ListSourceId);
            throw new ArgumentException("ScraperClassName cannot be null or empty", nameof(command));
        }

        // Strategy Pattern: Find the matching scraper from the injected collection
        var scraper = _scrapers.FirstOrDefault(s => s.GetType().Name == command.ScraperClassName);

        if (scraper == null)
        {
            _logger.LogError("Could not find an injected Scraper matching {ScraperClassName}. Did you register it in DI?", command.ScraperClassName);
            throw new InvalidOperationException($"Scraper class '{command.ScraperClassName}' not found in DI container.");
        }

        return scraper;
    }
}
