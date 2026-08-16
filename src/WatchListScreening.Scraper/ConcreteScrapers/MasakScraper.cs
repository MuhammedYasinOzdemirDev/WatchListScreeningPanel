using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Scraper.BaseScrapers;

namespace WatchListScreening.Scraper.ConcreteScrapers;

public class MasakScraper(ILogger<MasakScraper> logger) : BaseSeleniumScraper(logger)
{
    protected override async Task<List<RawScrapedItem>> ExecuteScrapingAsync(IWebDriver driver, HarvestCommandDto command, CancellationToken cancellationToken)
    {
        var items = new List<RawScrapedItem>();

        // 1. Navigate to URL
        driver.Navigate().GoToUrl(command.Url);

        // 2. Wait for page to load (Simulated wait for demonstration, should use WebDriverWait)
        await Task.Delay(3000, cancellationToken);

        // 3. Find target elements
        var elements = driver.FindElements(By.CssSelector(".sanction-row"));
        
        foreach (var element in elements)
        {
            // Throw if cancellation requested during iteration
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                items.Add(new RawScrapedItem
                {
                    RawFullName = element.FindElement(By.CssSelector(".name-col")).Text,
                    Country = element.FindElement(By.CssSelector(".country-col")).Text,
                    EntityTypeStr = "Individual" // Default or parse from column
                });
            }
            catch (NoSuchElementException ex)
            {
                Logger.LogWarning(ex, "Could not find expected columns in MASAK row.");
            }
        }

        Logger.LogInformation("MASAK Scraper successfully extracted {Count} items.", items.Count);
        return items;
    }
}
