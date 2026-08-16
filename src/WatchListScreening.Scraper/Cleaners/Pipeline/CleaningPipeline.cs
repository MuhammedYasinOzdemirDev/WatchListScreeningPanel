using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Application.Interfaces.Scraping;
using Microsoft.Extensions.Logging;

namespace WatchListScreening.Scraper.Cleaners.Pipeline;

public class CleaningPipeline(IEnumerable<ICleaningStep> steps, ILogger<CleaningPipeline> logger) : IDataCleaner
{
    private readonly IEnumerable<ICleaningStep> _steps = steps;
    private readonly ILogger<CleaningPipeline> _logger = logger;

    public CleanedItem Process(RawScrapedItem rawItem)
    {
        var currentItem = new CleanedItem
        {
            RawFullName = rawItem.RawFullName,
            CleanedFullName = rawItem.RawFullName, // initial
            Country = rawItem.Country,
            DateOfBirth = rawItem.DateOfBirth
        };

        foreach (var step in _steps)
        {
            try
            {
                currentItem = step.Process(rawItem, currentItem);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cleaning step {StepName} failed for item {RawName}", step.GetType().Name, rawItem.RawFullName);
            }
        }

        return currentItem;
    }

    public List<CleanedItem> ProcessBatch(List<RawScrapedItem> rawItems)
    {
        return rawItems.Select(Process).ToList();
    }
}
