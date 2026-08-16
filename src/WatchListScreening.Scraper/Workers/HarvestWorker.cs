using MassTransit;
using Microsoft.Extensions.Logging;
using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Application.Interfaces.Scraping;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Infrastructure.Data;

namespace WatchListScreening.Scraper.Workers;

public class HarvestWorker(
    ILogger<HarvestWorker> logger,
    IScraperFactory scraperFactory,
    IDataCleaner dataCleaner,
    AppDbContext dbContext) : IConsumer<HarvestCommandDto>
{
    private readonly ILogger<HarvestWorker> _logger = logger;
    private readonly IScraperFactory _scraperFactory = scraperFactory;
    private readonly IDataCleaner _dataCleaner = dataCleaner;
    private readonly AppDbContext _dbContext = dbContext;

    public async Task Consume(ConsumeContext<HarvestCommandDto> context)
    {
        var command = context.Message;
        _logger.LogInformation("Received Harvest Command for ListSourceId {ListSourceId}, Class: {ScraperClassName}", 
            command.ListSourceId, command.ScraperClassName);

        int totalScraped = 0;
        int totalInserted = 0;
        string? errorMessage = null;
        
        try
        {
            // 1. Get Scraper Implementation via Factory
            var scraper = _scraperFactory.Create(command);

            // 2. Execute Scraping (Polly can be wrapped here or inside scraper)
            var rawItems = await scraper.ScrapeAsync(command, context.CancellationToken);
            totalScraped = rawItems.Count;
            _logger.LogInformation("Successfully scraped {Count} raw items from {Url}", totalScraped, command.Url);

            if (totalScraped > 0)
            {
                // 3. Clean and Transform Data (Pipeline)
                var cleanedItems = _dataCleaner.ProcessBatch(rawItems);
                _logger.LogInformation("Successfully cleaned and normalized {Count} items", cleanedItems.Count);

                // 4. Map to Entity & Bulk Insert (Real DB Action)
                var entities = cleanedItems.Select(c => new HarvestedEntry
                {
                    ListSourceId = command.ListSourceId,
                    ListSourceRunId = command.ListSourceRunId,
                    RawFullName = c.RawFullName,
                    CleanedFullName = c.CleanedFullName,
                    Country = c.Country,
                    DateOfBirth = c.DateOfBirth,
                    AdditionalData = c.EntityType.HasValue ? $"{{\"EntityType\": \"{c.EntityType.Value}\"}}" : null,
                    IsProcessed = false,
                    ContentHash = string.IsNullOrWhiteSpace(c.ContentHash) ? Guid.NewGuid().ToString() : c.ContentHash,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                try
                {
                    await _dbContext.HarvestedEntries.AddRangeAsync(entities, context.CancellationToken);
                    totalInserted = await _dbContext.SaveChangesAsync(context.CancellationToken);
                    _logger.LogInformation("Harvest operation completed and saved {Count} records for {ListSourceId}", totalInserted, command.ListSourceId);
                }
                catch (Exception dbEx)
                {
                    // For example, if Postgres Unique Constraint triggers because ContentHash exists
                    _logger.LogWarning(dbEx, "Database insert conflict or failure for ListSourceId {ListSourceId}. Partial data or duplicates might exist.", command.ListSourceId);
                    errorMessage = "Database Insert Warning/Conflict: " + dbEx.Message;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Harvest operation failed for ListSourceId {ListSourceId}", command.ListSourceId);
            errorMessage = ex.Message;
        }
        finally
        {
            // 5. Publish Result Event
            await context.Publish(new HarvestResultEvent
            {
                ListSourceId = command.ListSourceId,
                ListSourceRunId = command.ListSourceRunId,
                IsSuccess = string.IsNullOrEmpty(errorMessage),
                TotalScraped = totalScraped,
                TotalInserted = totalInserted,
                ErrorMessage = errorMessage,
                CompletedAt = DateTime.UtcNow
            }, context.CancellationToken);
            
            _logger.LogInformation("Published HarvestResultEvent for ListSourceId {ListSourceId}", command.ListSourceId);
        }
    }
}
