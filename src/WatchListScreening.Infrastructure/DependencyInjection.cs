using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using StackExchange.Redis;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Application.Interfaces.Services;
using WatchListScreening.Infrastructure.BackgroundJobs;
using WatchListScreening.Infrastructure.Caching;
using WatchListScreening.Infrastructure.Data;
using WatchListScreening.Infrastructure.Data.Repositories;

namespace WatchListScreening.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // --- Veritabanı ---
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // --- Repository'ler ---
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISanctionEntryRepository, SanctionEntryRepository>();
        services.AddScoped<IScreeningResultRepository, ScreeningResultRepository>();

        // --- Redis ---
        var redisConn = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
        services.AddSingleton<ICacheService, RedisCacheService>();

        // --- Quartz Background Jobs ---
        services.AddQuartz(q =>
        {
            // ListUpdateJob — her 5 dakikada bir
            var listUpdateKey = new JobKey("ListUpdateJob");
            q.AddJob<ListUpdateJob>(opts => opts.WithIdentity(listUpdateKey));
            q.AddTrigger(opts => opts
                .ForJob(listUpdateKey)
                .WithIdentity("ListUpdateJob-trigger")
                .WithCronSchedule("0 */5 * * * ?"));

            // StaleScreeningCleanupJob — her 2 dakikada bir (dev ortamı)
            var cleanupKey = new JobKey("StaleScreeningCleanupJob");
            q.AddJob<StaleScreeningCleanupJob>(opts => opts.WithIdentity(cleanupKey));
            q.AddTrigger(opts => opts
                .ForJob(cleanupKey)
                .WithIdentity("StaleScreeningCleanupJob-trigger")
                .WithCronSchedule("0 */2 * * * ?"));

            // DashboardStatsJob — her 5 dakikada bir
            var statsKey = new JobKey("DashboardStatsJob");
            q.AddJob<DashboardStatsJob>(opts => opts.WithIdentity(statsKey));
            q.AddTrigger(opts => opts
                .ForJob(statsKey)
                .WithIdentity("DashboardStatsJob-trigger")
                .WithCronSchedule("0 */5 * * * ?"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
