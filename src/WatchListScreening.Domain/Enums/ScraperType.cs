namespace WatchListScreening.Domain.Enums;

/// <summary>
/// Defines which scraping strategy to use for a given source.
/// Http  = Static HTML via HtmlAgilityPack
/// Selenium = JavaScript-rendered pages via ChromeDriver
/// Api  = JSON/XML REST API endpoints
/// File = Direct file download (XML, CSV)
/// </summary>
public enum ScraperType
{
    Http = 1,
    Selenium = 2,
    Api = 3,
    File = 4
}
