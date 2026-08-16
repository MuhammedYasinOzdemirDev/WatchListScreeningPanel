using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Scraper.Cleaners.Pipeline;

public interface ICleaningStep
{
    CleanedItem Process(RawScrapedItem rawItem, CleanedItem currentItem);
}
