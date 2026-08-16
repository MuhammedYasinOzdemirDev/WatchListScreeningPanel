using System.Globalization;
using System.Text;
using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Scraper.Cleaners.Pipeline;

/// <summary>
/// Normalizes unicode characters (e.g., removes diacritics) and converts to uppercase.
/// "HÄSSAN" -> "HASSAN"
/// </summary>
public class UnicodeNormalizerStep : ICleaningStep
{
    public CleanedItem Process(RawScrapedItem rawItem, CleanedItem currentItem)
    {
        currentItem.CleanedFullName = Normalize(currentItem.CleanedFullName) ?? currentItem.CleanedFullName;
        currentItem.CleanedFirstName = Normalize(currentItem.CleanedFirstName);
        currentItem.CleanedLastName = Normalize(currentItem.CleanedLastName);

        return currentItem;
    }

    private string? Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        for (int i = 0; i < normalizedString.Length; i++)
        {
            char c = normalizedString[i];
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant().Trim();
    }
}
