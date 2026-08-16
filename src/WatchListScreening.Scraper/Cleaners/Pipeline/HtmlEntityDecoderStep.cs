using System.Net;
using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Scraper.Cleaners.Pipeline;

/// <summary>
/// Decodes HTML entities (e.g. &amp; to &, &quot; to ") in raw scraped fields.
/// </summary>
public class HtmlEntityDecoderStep : ICleaningStep
{
    public CleanedItem Process(RawScrapedItem rawItem, CleanedItem currentItem)
    {
        if (!string.IsNullOrWhiteSpace(currentItem.CleanedFullName))
        {
            currentItem.CleanedFullName = WebUtility.HtmlDecode(currentItem.CleanedFullName);
        }

        if (!string.IsNullOrWhiteSpace(rawItem.RawFirstName))
        {
            currentItem.CleanedFirstName = WebUtility.HtmlDecode(rawItem.RawFirstName);
        }

        if (!string.IsNullOrWhiteSpace(rawItem.RawLastName))
        {
            currentItem.CleanedLastName = WebUtility.HtmlDecode(rawItem.RawLastName);
        }

        return currentItem;
    }
}
