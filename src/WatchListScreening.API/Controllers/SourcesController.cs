using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Infrastructure.Data;
using Hangfire;
using WatchListScreening.API.Jobs;

namespace WatchListScreening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sources = await _dbContext.ListSources.ToListAsync();
        return Ok(sources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var source = await _dbContext.ListSources.FindAsync(id);
        if (source == null) return NotFound();
        return Ok(source);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ListSource source)
    {
        source.CreatedAt = DateTime.UtcNow;
        _dbContext.ListSources.Add(source);
        await _dbContext.SaveChangesAsync();

        // Eğer Cron ifade varsa Hangfire Job oluştur
        UpdateHangfireJob(source);

        return CreatedAtAction(nameof(Get), new { id = source.Id }, source);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ListSource sourceUpdate)
    {
        var source = await _dbContext.ListSources.FindAsync(id);
        if (source == null) return NotFound();

        source.Name = sourceUpdate.Name;
        source.Url = sourceUpdate.Url;
        source.Category = sourceUpdate.Category;
        source.ScraperType = sourceUpdate.ScraperType;
        source.ScraperConfig = sourceUpdate.ScraperConfig;
        source.CronExpression = sourceUpdate.CronExpression;
        source.IsActive = sourceUpdate.IsActive;
        source.HasScraperImpl = sourceUpdate.HasScraperImpl;
        source.ScraperClassName = sourceUpdate.ScraperClassName;
        source.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        UpdateHangfireJob(source);

        return Ok(source);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var source = await _dbContext.ListSources.FindAsync(id);
        if (source == null) return NotFound();

        _dbContext.ListSources.Remove(source);
        await _dbContext.SaveChangesAsync();

        var jobId = $"harvest-source-{source.Id}";
        RecurringJob.RemoveIfExists(jobId);

        return NoContent();
    }

    private void UpdateHangfireJob(ListSource source)
    {
        var jobId = $"harvest-source-{source.Id}";

        if (source.IsActive && source.HasScraperImpl && !string.IsNullOrWhiteSpace(source.CronExpression))
        {
            RecurringJob.AddOrUpdate<HarvestSchedulerJob>(
                jobId,
                job => job.TriggerHarvestAsync(source.Id),
                source.CronExpression
            );
        }
        else
        {
            RecurringJob.RemoveIfExists(jobId);
        }
    }
}
