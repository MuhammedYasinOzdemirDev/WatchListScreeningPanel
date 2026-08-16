using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Scraper.Cleaners.Pipeline;

public class CategoryClassifierStep : ICleaningStep
{
    private readonly string[] _corporateKeywords = { "LTD", "A.S.", "INC", "CORP", "LLC", "LIMITED", "COMPANY" };

    public CleanedItem Process(RawScrapedItem rawItem, CleanedItem currentItem)
    {
        // If entity type is already specified by scraper, don't override
        if (!string.IsNullOrEmpty(rawItem.EntityTypeStr))
        {
            if (Enum.TryParse<EntityType>(rawItem.EntityTypeStr, true, out var parsedType))
            {
                currentItem.EntityType = parsedType;
                return currentItem;
            }
        }

        // Auto-classify based on name
        var upperName = currentItem.CleanedFullName.ToUpperInvariant();
        if (_corporateKeywords.Any(k => upperName.Contains(k)))
        {
            currentItem.EntityType = EntityType.Organization;
        }
        else
        {
            currentItem.EntityType = EntityType.Person;
        }

        return currentItem;
    }
}
