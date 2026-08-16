using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Domain.Enums;
using WatchListScreening.Infrastructure.Data;

namespace WatchListScreening.API.Consumers;

public class HarvestResultConsumer(ILogger<HarvestResultConsumer> logger, AppDbContext dbContext) : IConsumer<HarvestResultEvent>
{
    private readonly ILogger<HarvestResultConsumer> _logger = logger;
    private readonly AppDbContext _dbContext = dbContext;

    public async Task Consume(ConsumeContext<HarvestResultEvent> context)
    {
        var result = context.Message;
        _logger.LogInformation("Harvest result received for ListSourceId {ListSourceId}, RunId: {RunId}, Success: {IsSuccess}", 
            result.ListSourceId, result.ListSourceRunId, result.IsSuccess);

        var run = await _dbContext.ListSourceRuns
            .FirstOrDefaultAsync(r => r.Id == result.ListSourceRunId, context.CancellationToken);

        if (run == null)
        {
            _logger.LogWarning("ListSourceRun with ID {RunId} not found. Cannot update status.", result.ListSourceRunId);
            return;
        }

        run.FinishedAt = result.CompletedAt;
        run.Status = result.IsSuccess ? HarvestStatus.Success : HarvestStatus.Failed;
        
        // Not: Kısmi başarı (PartialSuccess) vb. eklenebilir.
        if (result.IsSuccess && result.TotalInserted < result.TotalScraped)
        {
             run.Status = HarvestStatus.PartialSuccess;
        }

        run.TotalScraped = result.TotalScraped;
        run.TotalNew = result.TotalInserted; // Simple mapping for now
        run.ErrorMessage = result.ErrorMessage;

        // Ayrıca ListSource tablosundaki son durum ve tarih güncellenebilir
        var source = await _dbContext.ListSources.FindAsync([result.ListSourceId], context.CancellationToken);
        if (source != null)
        {
            source.LastHarvestAt = result.CompletedAt;
            source.LastHarvestStatus = run.Status;
            source.TotalRecordsHarvested += result.TotalInserted; // increment total
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Successfully updated ListSourceRun {RunId} status to {Status}", run.Id, run.Status);
    }
}
