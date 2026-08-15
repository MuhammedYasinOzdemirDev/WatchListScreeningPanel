using System.Text.Json;
using StackExchange.Redis;
using WatchListScreening.Application.Interfaces.Services;

namespace WatchListScreening.Infrastructure.Caching;

/// <summary>
/// ICacheService'in Redis implementasyonu (Infrastructure katmanı).
/// DI'da Singleton olarak kaydedilir — Redis bağlantısı uygulama boyunca tek olmalı.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue)
            return default; // Cache MISS

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        var ttl  = expiry ?? TimeSpan.FromMinutes(30);
        await _db.StringSetAsync(key, json, ttl);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
