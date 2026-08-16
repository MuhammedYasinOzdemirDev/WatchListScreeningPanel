using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Scraper.BaseScrapers;

namespace WatchListScreening.Scraper.ConcreteScrapers;

public class OfacScraper(ILogger<OfacScraper> logger, HttpClient httpClient) : BaseHttpScraper(logger, httpClient)
{
    public override async Task<List<RawScrapedItem>> ScrapeAsync(HarvestCommandDto command, CancellationToken cancellationToken = default)
    {
        var items = new List<RawScrapedItem>();
        Logger.LogInformation("Starting Http scrape for {Url}", command.Url);

        var response = await HttpClient.GetAsync(command.Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(rawContent);

        var nodes = doc.DocumentNode.SelectNodes("//table//tr[position()>1]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var tds = node.SelectNodes("td");
                if (tds != null && tds.Count >= 3)
                {
                    items.Add(new RawScrapedItem
                    {
                        RawFullName = tds[0].InnerText,
                        Country = tds[1].InnerText,
                        DateOfBirth = tds[2].InnerText
                    });
                }
            }
        }

        return items;
    }
}
