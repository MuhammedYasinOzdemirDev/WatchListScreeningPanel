using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Application.Interfaces.Scraping;

namespace WatchListScreening.Scraper.BaseScrapers;

public abstract class BaseFileScraper(ILogger logger) : ISourceScraper
{
    protected readonly ILogger Logger = logger;

    public abstract Task<List<RawScrapedItem>> ScrapeAsync(HarvestCommandDto command, CancellationToken cancellationToken = default);
}

public abstract class BaseApiScraper(ILogger logger, HttpClient httpClient) : ISourceScraper
{
    protected readonly ILogger Logger = logger;
    protected readonly HttpClient HttpClient = httpClient;

    public abstract Task<List<RawScrapedItem>> ScrapeAsync(HarvestCommandDto command, CancellationToken cancellationToken = default);
}

public abstract class BaseHttpScraper(ILogger logger, HttpClient httpClient) : ISourceScraper
{
    protected readonly ILogger Logger = logger;
    protected readonly HttpClient HttpClient = httpClient;

    public abstract Task<List<RawScrapedItem>> ScrapeAsync(HarvestCommandDto command, CancellationToken cancellationToken = default);
}

public abstract class BaseSeleniumScraper(ILogger logger) : ISourceScraper
{
    protected readonly ILogger Logger = logger;

    // Template Method Pattern: Defines the skeleton of the algorithm
    public async Task<List<RawScrapedItem>> ScrapeAsync(HarvestCommandDto command, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Starting Selenium scrape for {Url}", command.Url);

        // TODO: Factory or Pool should manage WebDrivers. For now, creating a local headless instance.
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--headless");
        chromeOptions.AddArgument("--disable-gpu");
        chromeOptions.AddArgument("--no-sandbox");

        using IWebDriver driver = new ChromeDriver(chromeOptions);

        try
        {
            // Apply Timeout from command
            if (command.TimeoutSeconds > 0)
            {
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(command.TimeoutSeconds);
            }

            return await ExecuteScrapingAsync(driver, command, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Selenium scraping failed for {Url}. Error: {Message}", command.Url, ex.Message);
            throw;
        }
        finally
        {
            driver.Quit();
        }
    }

    // Subclasses MUST implement this specific scraping logic
    protected abstract Task<List<RawScrapedItem>> ExecuteScrapingAsync(IWebDriver driver, HarvestCommandDto command, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a dynamic list of actions (Navigate, Click, Wait, Input) based on the config.
    /// Used by concrete scrapers to navigate complex SPAs before reading rows.
    /// </summary>
    protected async Task ExecuteScenarioAsync(IWebDriver driver, ScrapingConfig config, CancellationToken cancellationToken)
    {
        if (config.Steps == null || !config.Steps.Any()) return;

        foreach (var step in config.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (step.Action.ToLowerInvariant())
            {
                case "navigate":
                    if (!string.IsNullOrEmpty(step.Target))
                    {
                        Logger.LogInformation("Scenario: Navigating to {Target}", step.Target);
                        driver.Navigate().GoToUrl(step.Target);
                    }
                    break;
                case "wait":
                    var waitMs = step.DurationMs ?? 1000;
                    Logger.LogInformation("Scenario: Waiting for {Ms} ms", waitMs);
                    await Task.Delay(waitMs, cancellationToken);
                    break;
                case "click":
                    if (!string.IsNullOrEmpty(step.Target))
                    {
                        Logger.LogInformation("Scenario: Clicking on {Target}", step.Target);
                        driver.FindElement(By.CssSelector(step.Target)).Click();
                    }
                    break;
                case "input":
                    if (!string.IsNullOrEmpty(step.Target) && !string.IsNullOrEmpty(step.Value))
                    {
                        Logger.LogInformation("Scenario: Typing into {Target}", step.Target);
                        driver.FindElement(By.CssSelector(step.Target)).SendKeys(step.Value);
                    }
                    break;
                default:
                    Logger.LogWarning("Scenario: Unknown action '{Action}'", step.Action);
                    break;
            }
        }
    }
}
