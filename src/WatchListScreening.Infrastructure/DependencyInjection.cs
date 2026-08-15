using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Application.Interfaces.Services;
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

        // --- Repository'ler (Scoped: her HTTP isteği için yeni instance) ---
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISanctionEntryRepository, SanctionEntryRepository>();
        services.AddScoped<IScreeningResultRepository, ScreeningResultRepository>();

        // --- Redis Cache (Singleton: bağlantı uygulama ömrü boyunca tek olmalı) ---
        var redisConnectionString = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
