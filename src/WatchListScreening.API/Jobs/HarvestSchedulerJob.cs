using Hangfire;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Domain.Enums;
using WatchListScreening.Infrastructure.Data;

namespace WatchListScreening.API.Jobs;

public class HarvestSchedulerJob(ILogger<HarvestSchedulerJob> logger, AppDbContext dbContext, IPublishEndpoint publishEndpoint)
{
    private readonly ILogger<HarvestSchedulerJob> _logger = logger;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    [AutomaticRetry(Attempts = 0)]
    public async Task TriggerHarvestAsync(int listSourceId)
    {
        _logger.LogInformation("Scheduled job triggered for ListSourceId {ListSourceId}", listSourceId);

        var source = await _dbContext.ListSources
            .FirstOrDefaultAsync(s => s.Id == listSourceId);

        if (source == null)
        {
            _logger.LogWarning("ListSource {ListSourceId} not found.", listSourceId);
            return;
        }

        if (!source.IsActive || !source.HasScraperImpl)
        {
            _logger.LogWarning("ListSource {ListSourceId} is not active or missing scraper implementation. Skipping.", listSourceId);
            return;
        }

        // 1. Create a new Run record
        var run = new ListSourceRun
        {
            ListSourceId = source.Id,
            StartedAt = DateTime.UtcNow,
            Status = HarvestStatus.Running,
            TriggeredBy = "Hangfire Scheduler"
        };

        await _dbContext.ListSourceRuns.AddAsync(run);
        await _dbContext.SaveChangesAsync();

        // 2. Build the command
        var command = new HarvestCommandDto
        {
            ListSourceId = source.Id,
            ListSourceRunId = run.Id, // Pass the new ID to the scraper
            Url = source.Url,
            ScraperType = source.ScraperType,
            ScraperClassName = source.ScraperClassName,
            ScraperConfigJson = source.ScraperConfig
        };

        // 3. Publish to RabbitMQ
        await _publishEndpoint.Publish(command);
        
        _logger.LogInformation("HarvestCommand published to RabbitMQ for Source {SourceId}, Run {RunId}", source.Id, run.Id);
    }
}
