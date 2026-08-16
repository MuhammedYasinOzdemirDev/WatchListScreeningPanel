using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Scraper.Cleaners.Pipeline;

/// <summary>
/// Splits CleanedFullName into CleanedFirstName and CleanedLastName if they are not provided by the scraper.
/// Assumes the last word is the last name, and the rest is the first name.
/// </summary>
public class NameSplitterStep : ICleaningStep
{
    public CleanedItem Process(RawScrapedItem rawItem, CleanedItem currentItem)
    {
        if (!string.IsNullOrWhiteSpace(currentItem.CleanedFullName))
        {
            if (string.IsNullOrWhiteSpace(currentItem.CleanedFirstName) && string.IsNullOrWhiteSpace(currentItem.CleanedLastName))
            {
                var parts = currentItem.CleanedFullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    currentItem.CleanedLastName = parts.Last();
                    currentItem.CleanedFirstName = string.Join(" ", parts.Take(parts.Length - 1));
                }
                else
                {
                    currentItem.CleanedFirstName = currentItem.CleanedFullName;
                }
            }
        }

        return currentItem;
    }
}
