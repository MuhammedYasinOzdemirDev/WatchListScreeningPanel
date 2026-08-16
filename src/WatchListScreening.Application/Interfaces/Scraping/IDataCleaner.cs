using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Application.Interfaces.Scraping;

public interface IDataCleaner
{
    CleanedItem Process(RawScrapedItem rawItem);
    List<CleanedItem> ProcessBatch(List<RawScrapedItem> rawItems);
}
