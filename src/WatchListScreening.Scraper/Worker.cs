using WatchListScreening.Application.Interfaces.Scraping;

namespace WatchListScreening.Scraper;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IScraperFactory _scraperFactory;
    private readonly IDataCleaner _dataCleaner;

    public Worker(ILogger<Worker> logger, IScraperFactory scraperFactory, IDataCleaner dataCleaner)
    {
        _logger = logger;
        _scraperFactory = scraperFactory;
        _dataCleaner = dataCleaner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scraper Worker Service başladı: {time}", DateTimeOffset.Now);

        // Gelecek aşamada RabbitMQ (MassTransit) üzerinden dinlemeye başlayacağız.
        // Şimdilik sadece servisin ayakta kalmasını sağlıyoruz.
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Scraper Worker dinlemede: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(60000, stoppingToken);
        }
    }
}
