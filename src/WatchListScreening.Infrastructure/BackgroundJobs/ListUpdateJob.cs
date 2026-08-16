using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Application.Interfaces.Services;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class ListUpdateJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICacheService _cache;
    private readonly ILogger<ListUpdateJob> _logger;

    private static readonly string[] SimulatedNames =
    [
        "Viktor Petrov", "Chen Wei", "Omar Al-Rashid",
        "Dmitri Volkov", "Hassan Al-Farsi", "Ivan Kozlov"
    ];
    private static readonly string[] Sources = ["OFAC", "UN", "EU", "MASAK"];

    public ListUpdateJob(IServiceScopeFactory scopeFactory, ICacheService cache, ILogger<ListUpdateJob> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[ListUpdateJob] Başladı: {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var random = new Random();
        var count = random.Next(1, 4);
        var added = 0;

        for (int i = 0; i < count; i++)
        {
            var name = SimulatedNames[random.Next(SimulatedNames.Length)];
            var source = Sources[random.Next(Sources.Length)];

            var existing = await unitOfWork.SanctionEntries.SearchByNameAsync(name);
            if (existing.Any(e => e.IsActive))
                continue;

            await unitOfWork.SanctionEntries.AddAsync(new SanctionEntry
            {
                FullName     = name,
                EntityType   = EntityType.Person,
                // ListSource kaldırıldı — Faz 2'de ListSourceId FK ile yönetilecek
                IsActive     = true,
                AddedAt      = DateTime.UtcNow,
                CreatedAt    = DateTime.UtcNow
            });
            added++;
        }

        if (added == 0)
        {
            _logger.LogInformation("[ListUpdateJob] Eklenecek yeni kayıt yok.");
            return;
        }

        await unitOfWork.SaveChangesAsync();
        await _cache.RemoveAsync("sanctions:all");
        await _cache.RemoveAsync("dashboard:stats");

        await unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            Action      = "ListUpdate",
            EntityType  = "SanctionEntry",
            PerformedBy = "System",
            // PerformedAt kaldırıldı — BaseEntity.CreatedAt kullanılıyor
            Details     = $"{added} yeni kayıt eklendi.",
            CreatedAt   = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        _logger.LogInformation("[ListUpdateJob] {Count} kayıt eklendi.", added);
    }
}
