using Microsoft.Extensions.Logging;
using Quartz;
using WatchListScreening.Application.Interfaces.Services;

namespace WatchListScreening.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class DashboardStatsJob : IJob
{
    private readonly ICacheService _cache;
    private readonly ILogger<DashboardStatsJob> _logger;

    public DashboardStatsJob(ICacheService cache, ILogger<DashboardStatsJob> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[DashboardStatsJob] Cache yenileniyor: {Time}", DateTime.UtcNow);
        await _cache.RemoveAsync("dashboard:stats");
        _logger.LogInformation("[DashboardStatsJob] dashboard:stats temizlendi.");
    }
}
