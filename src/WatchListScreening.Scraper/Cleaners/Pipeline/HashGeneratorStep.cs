using System.Security.Cryptography;
using System.Text;
using WatchListScreening.Application.DTOs.Harvest;

namespace WatchListScreening.Scraper.Cleaners.Pipeline;

/// <summary>
/// Generates a SHA256 ContentHash based on (CleanedFullName + ListSourceId + DateOfBirth + NationalId).
/// This hash is used by the Worker to enforce deduplication against the Database.
/// </summary>
public class HashGeneratorStep : ICleaningStep
{
    public CleanedItem Process(RawScrapedItem rawItem, CleanedItem currentItem)
    {
        var rawKey = $"{currentItem.CleanedFullName}_{currentItem.DateOfBirth}_{rawItem.NationalId}".ToUpperInvariant();
        
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawKey);
        var hashBytes = sha256.ComputeHash(bytes);
        
        var sb = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("X2"));
        }
        
        currentItem.ContentHash = sb.ToString();

        return currentItem;
    }
}
