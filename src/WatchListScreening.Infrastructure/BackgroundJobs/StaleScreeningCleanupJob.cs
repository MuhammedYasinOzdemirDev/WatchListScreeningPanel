using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class StaleScreeningCleanupJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StaleScreeningCleanupJob> _logger;

    public StaleScreeningCleanupJob(IServiceScopeFactory scopeFactory, ILogger<StaleScreeningCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[StaleScreeningCleanupJob] Başladı: {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        var staleRequests = unitOfWork.ScreeningRequests
            .Query()
            .Where(r => r.Status == ScreeningStatus.Completed
                     && r.CompletedAt.HasValue
                     && r.CompletedAt.Value < cutoffDate)
            .ToList();

        if (staleRequests.Count == 0)
        {
            _logger.LogInformation("[StaleScreeningCleanupJob] Silinecek kayıt yok.");
            return;
        }

        foreach (var request in staleRequests)
            unitOfWork.ScreeningRequests.Delete(request);

        await unitOfWork.SaveChangesAsync();
        _logger.LogInformation("[StaleScreeningCleanupJob] {Count} eski tarama silindi.", staleRequests.Count);
    }
}
