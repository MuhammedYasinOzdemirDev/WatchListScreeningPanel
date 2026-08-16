using System.Text.RegularExpressions;
using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Scraper.Cleaners.Pipeline;

/// <summary>
/// Removes extra whitespaces, punctuation marks, and unwanted characters.
/// </summary>
public class NameNormalizerStep : ICleaningStep
{
    // Sadece harf, rakam ve tekil boşluklara izin verir.
    private static readonly Regex UnwantedCharsRegex = new Regex(@"[^\w\s\d]", RegexOptions.Compiled);
    private static readonly Regex ExtraSpacesRegex = new Regex(@"\s+", RegexOptions.Compiled);

    public CleanedItem Process(RawScrapedItem rawItem, CleanedItem currentItem)
    {
        currentItem.CleanedFullName = Clean(currentItem.CleanedFullName) ?? currentItem.CleanedFullName;
        currentItem.CleanedFirstName = Clean(currentItem.CleanedFirstName);
        currentItem.CleanedLastName = Clean(currentItem.CleanedLastName);

        return currentItem;
    }

    private string? Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var cleaned = UnwantedCharsRegex.Replace(text, " ");
        cleaned = ExtraSpacesRegex.Replace(cleaned, " ");

        return cleaned.Trim();
    }
}
