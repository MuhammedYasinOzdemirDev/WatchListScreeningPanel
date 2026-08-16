using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using WatchListScreening.Application.DTOs.Harvest;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Domain.Enums;
using WatchListScreening.Infrastructure.Data;

namespace WatchListScreening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HarvestController(AppDbContext dbContext, IPublishEndpoint publishEndpoint) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    [HttpPost("trigger/{listSourceId}")]
    public async Task<IActionResult> TriggerHarvest(int listSourceId)
    {
        var source = await _dbContext.ListSources.FindAsync(listSourceId);
        
        if (source == null) return NotFound("ListSource not found");
        if (!source.IsActive) return BadRequest("Source is disabled");
        if (!source.HasScraperImpl) return BadRequest("Source has no scraper implementation");

        var run = new ListSourceRun
        {
            ListSourceId = source.Id,
            StartedAt = DateTime.UtcNow,
            Status = HarvestStatus.Running,
            TriggeredBy = "Manual (API)"
        };

        _dbContext.ListSourceRuns.Add(run);
        await _dbContext.SaveChangesAsync();

        var command = new HarvestCommandDto
        {
            ListSourceId = source.Id,
            ListSourceRunId = run.Id,
            Url = source.Url,
            ScraperType = source.ScraperType,
            ScraperClassName = source.ScraperClassName,
            ScraperConfigJson = source.ScraperConfig
        };

        await _publishEndpoint.Publish(command);

        return Accepted(new { message = "Harvest triggered", runId = run.Id });
    }

    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns([FromQuery] int? listSourceId, [FromQuery] int take = 50)
    {
        var query = _dbContext.ListSourceRuns.AsQueryable();

        if (listSourceId.HasValue)
        {
            query = query.Where(r => r.ListSourceId == listSourceId.Value);
        }

        var runs = await query
            .OrderByDescending(r => r.StartedAt)
            .Take(take)
            .ToListAsync();

        return Ok(runs);
    }
}
